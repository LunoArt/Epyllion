using System;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Luno.Epyllion.Editor.UI
{
    public class EpyllionWindow : EditorWindow, IHasCustomMenu
    {
        class Styles
        {
            public GUIStyle lockButton = "IN LockButton";
        }
        private static Styles s_Styles;
        
        [MenuItem("Window/Luno/Epyllion")]
        public static void ShowWindow()
        {
            GetWindow<EpyllionWindow>("Epyllion");
        }

        private Graph _graph;
        private bool _locked;

        private void OnEnable()
        {
            var root = rootVisualElement;
            
            //Adding the style
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Packages/com.lunoart.epyllion/Editor/Resources/EpyllionWindow.uss");
            root.styleSheets.Add(styleSheet);
            
            //Adding the UXML
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.lunoart.epyllion/Editor/Resources/EpyllionWindow.uxml").CloneTree(root);
            //root.Add(visualTree);

            _graph = root.Q<Graph>("graph");

            if (_graph == null)
            {
                throw new Exception	("Couldn't find the Graph element");
            }
            
            root.Q<UnityEngine.UIElements.Button>("createButton").RegisterCallback<MouseUpEvent>(evt => CreateStory());
            
            //OnSelectionChange();
            OnGUI();
        }

        private void OnSelectionChange()
        {
            /*var story = Selection.activeObject as Story;
            _graph.story = story;*/
            OnGUI();
        }

        private void OnGUI()
        {
            if (_locked && _graph.story == null)
                _locked = false;
            
            var story = _locked ? _graph.story : Selection.activeObject as Story;
            if (story != _graph.story || story == null)
                _graph.story = story;
        }

        private void CreateStory()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Story", "New Epyllion Story", "asset",
                "Set a location to save the asset");
            Story story = ScriptableObject.CreateInstance<Story>();
            AssetDatabase.CreateAsset(story,path);
            UnityEditor.Selection.activeObject = story;
        }


        private static bool sceneLoadedForEdit = false;
        private static bool sceneActivatedForEdit = false;
        private static Scene? originalScene;
        internal static void BeginSceneEdit(SceneAsset sceneAsset)
        {
            originalScene = EditorSceneManager.GetActiveScene();
            var scene = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(sceneAsset));
            if (!scene.isLoaded)
            {
                sceneLoadedForEdit = true;
                scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset),
                    OpenSceneMode.Additive);
            }

            if (EditorSceneManager.GetActiveScene() == scene) return;
            sceneActivatedForEdit = true;
            EditorSceneManager.SetActiveScene(scene);
        }

        internal static void EndSceneEdit()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (originalScene != null)
            {
                if (sceneActivatedForEdit)
                {
                    EditorSceneManager.SetActiveScene((Scene) originalScene);
                }

                if (sceneLoadedForEdit)
                {
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            originalScene = null;
            sceneActivatedForEdit = false;
            sceneLoadedForEdit = false;
        }

        internal static SceneStoryManager GetSceneManager()
        {
            var managers = FindObjectsOfType<SceneStoryManager>();
            foreach (var manager in managers)
            {
                if (manager.gameObject.scene == SceneManager.GetActiveScene())
                    return manager;
            }
            var newManager = new GameObject("SceneStoryManager").AddComponent<SceneStoryManager>();
            //newManager.gameObject.hideFlags = HideFlags.HideInHierarchy;
            return newManager;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorSceneManager.sceneOpened += (Scene scene, OpenSceneMode mode) => SceneLoaded();
            
            // on Editor opened, bind in the next frame so the objects are loaded
            EditorApplication.delayCall += SceneLoaded;
        }
        
        private static void SceneLoaded()
        {
            var managers = FindObjectsOfType<SceneStoryManager>();
            foreach (var manager in managers)
            {
                manager.BindActions();
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(EditorGUIUtility.TrTextContent("Lock"), _locked, () => { _locked = !_locked;});
        }

        protected virtual void ShowButton(Rect rect)
        {
            if(s_Styles == null)
                s_Styles = new Styles();

            EditorGUI.BeginChangeCheck();
            _locked = GUI.Toggle(rect, _locked, GUIContent.none, s_Styles.lockButton);

            if (!EditorGUI.EndChangeCheck()) return;
            if (!_locked)
            {
                OnGUI();
            }
        }
    }
}