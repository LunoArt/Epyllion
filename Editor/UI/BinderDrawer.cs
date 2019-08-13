using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
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

                    UnityEditor.Editor.CreateEditor(action.objectReferenceValue).OnInspectorGUI();

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