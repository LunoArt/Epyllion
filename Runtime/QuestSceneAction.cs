using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestSceneAction : QuestAction
    {
        [SerializeField] internal QuestSceneActionWrapper _wrapper;

        internal override void Complete()
        {
            base.Complete();
            _wrapper.Complete();
        }

        private void OnDestroy()
        {
            _wrapper.initialized = false;
        }
    }
}