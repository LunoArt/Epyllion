using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
        public QuestAction[] actions = new QuestAction[0];
    }

    [CustomPropertyDrawer(typeof(NodeBinder))]
    internal class BinderDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new IMGUIContainer(() =>
            {
                var actions = property.FindPropertyRelative("actions");
                for (int i = 0; i < actions.arraySize; i++)
                {
                    EditorGUILayout.Space();
                    var action = actions.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    Editor.CreateEditor(action.objectReferenceValue).OnInspectorGUI();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove Action"))
                    {
                        actions.DeleteArrayElementAtIndex(i);
                        actions.DeleteArrayElementAtIndex(i);
                        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

            });

          
            return container;
        }
    }
}