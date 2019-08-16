using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Button = UnityEngine.UI.Button;

namespace Luno.Epyllion.Editor.UI
{
    public class EpyllionWindow : EditorWindow
    {
        [MenuItem("Window/Luno/Epyllion")]
        public static void ShowWindow()
        {
            GetWindow<EpyllionWindow>("Epyllion");
        }

        private Graph _graph;

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
            
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            GameObject managerObject = Selection.activeGameObject;
            Story story;
            StorySceneManager manager;
            if (managerObject != null)
            {
                manager = managerObject.GetComponent<StorySceneManager>();
                if (manager != null)
                {
                    story = manager.story;
                    if (story != null)
                    {
                        _graph.story = story;
                        //_graph.SetTargets(story,manager);
                        return;
                    }
                }
            }
            else
            {
                story = Selection.activeObject as Story;
                if (story != null)
                {
                    _graph.story = story;
                    //_graph.SetTargets(story, null);
                    return;
                }
            }

            _graph.story = null;
            //_graph.SetTargets(null,null);
        }

        private void CreateStory()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Story", "New Epyllion Story", "asset",
                "Set a location to save the asset");
            Story story = ScriptableObject.CreateInstance<Story>();
            AssetDatabase.CreateAsset(story,path);
            UnityEditor.Selection.activeObject = story;
        }
    }
}