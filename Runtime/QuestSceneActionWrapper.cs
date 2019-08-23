using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Luno.Epyllion
{
    internal class QuestSceneActionWrapper : QuestAction
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        internal QuestSceneAction _action;
        [SerializeField] internal string _scenePath;
        [SerializeField] internal string _actionTypeName;
        [SerializeField] internal Object _sceneAsset;
        [SerializeField] internal Object _actionType;

        
        
        public override void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if (_action != null)
                _action.OnQuestStateChange(newState, oldState);
            else
            {
                var type = Type.GetType(_actionTypeName);
                var method = type.GetMethod("OnQuestStateChangeOutOfScene",
                    BindingFlags.Static | BindingFlags.Public, null, new [] {typeof(string), typeof(QuestState), typeof(QuestState) }, null);
                method?.Invoke(null, new object[] {_scenePath, newState, oldState});
            }
        }

        public override void OnSetup()
        {
            if (_action != null)
            {
                _action.OnSetup();
            }
            else
            {
                var method = Type.GetType(_actionTypeName).GetMethod("OnSetupOutOfScene",
                    BindingFlags.Static | BindingFlags.Public, null, new [] {typeof(string)}, null);
                method?.Invoke(null, new object[] {_scenePath});
            }
        }

        public bool IsSceneActive()
        {
            return _action != null;
        }

#if UNITY_EDITOR
        public MonoScript actionType => _actionType as MonoScript;
        public SceneAsset sceneAsset => _sceneAsset as SceneAsset;

        public void OnBeforeSerialize()
        {
            _scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
            _actionTypeName = actionType.GetClass().AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {}
#endif
    }
}