using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestSceneAction : QuestAction
    {
        [SerializeField] internal QuestSceneActionWrapper _wrapper;

        public override void Complete()
        {
            base.Complete();
            _wrapper.Complete();
        }
    }
}