using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestSceneAction : QuestAction
    {
        [SerializeField] [HideInInspector] internal QuestSceneActionWrapper _wrapper;
        public QuestSceneActionWrapper wrapper => _wrapper;

        public override void Complete(string result = null)
        {
            base.Complete(result);
            wrapper.Complete(result);
        }
    }
}