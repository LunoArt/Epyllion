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
            foreach (var action in _actions)
            {
                action._wrapper._action = action;
            }
        }

        private void OnEnable()
        {
            foreach (var action in _actions)
            {
                action._wrapper.Initialize();
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
        
        public void QuestDeleted(Quest quest)
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