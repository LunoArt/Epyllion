using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
            
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            GameObject storyObject = Selection.activeObject as GameObject;
            if (storyObject == null)
                return;
            Story story = storyObject.GetComponent<Story>();
            if (story == null)
            {
                _graph.quest = null;
                return;
            }
            _graph.quest = story.quest;
        }
    }
}