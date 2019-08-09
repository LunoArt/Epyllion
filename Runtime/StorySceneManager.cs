using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Luno.Epyllion
{
    public class StorySceneManager: MonoBehaviour
    {
        public StoryStructure storyStructure;
        [SerializeField]
        internal NodeBinder[] binders = new NodeBinder[0];
    }
    
    [Serializable]
    internal class NodeBinder
    {
        public int id;
        public QuestAction action;
    }

    [Serializable]
    internal class QuestAction : UnityEvent {}

    [CustomPropertyDrawer(typeof(NodeBinder))]
    internal class BinderDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            
            var actionField = new PropertyField(property.FindPropertyRelative("action"));
            container.Add(actionField);
            return container;
        }
    }
}