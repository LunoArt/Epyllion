using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public class SceneStoryManager : MonoBehaviour
    {
        [SerializeField] internal List<QuestSceneAction> _actions = new List<QuestSceneAction>();

        private void Awake()
        {
            BindActions();
        }

        private void OnEnable()
        {
            foreach (var action in _actions)
            {
                if(action._quest._story.initialized)
                    action._wrapper.OnSetup();
            }
        }

        internal void BindActions()
        {
            foreach (var action in _actions)
            {
                action._wrapper._action = action;
            }
        }

#if UNITY_EDITOR
        internal void WrapperDeleted(QuestSceneActionWrapper wrapper)
        {
            for (var a = _actions.Count - 1; a >= 0; a--)
            {
                var action = _actions[a];
                if (action._wrapper == wrapper)
                    _actions.RemoveAt(a);
            }

            EditorUtility.SetDirty(this);
        }
        
        internal void QuestDeleted(Quest quest)
        {
            for (var a = _actions.Count - 1; a >= 0; a--)
            {
                var action = _actions[a];
                if (action._quest == quest)
                    _actions.RemoveAt(a);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}