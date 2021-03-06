using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Luno.Epyllion.Editor.UI
{
    [CustomEditor(typeof(QuestSceneActionWrapper))]
    public class QuesSceneActionWrapperDrawer : UnityEditor.Editor
    {
        private UnityEditor.Editor _actionEditor;

        public override void OnInspectorGUI()
        {
            var wrapper = (QuestSceneActionWrapper) target;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sceneAsset"));

            if (!EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(wrapper._sceneAsset)).isLoaded)
            {
                GUILayout.Label("Open scene to see the action");
                return;
            }
            
            if (_actionEditor == null || _actionEditor.target != wrapper._action)
            {
                _actionEditor = CreateEditor(wrapper._action);
            }
            if(_actionEditor != null)
                _actionEditor.OnInspectorGUI();
        }
    }
}