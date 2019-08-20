using System;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestAction : ScriptableObject
    {
        public bool completed { get; private set; }

        [SerializeField] [HideInInspector] internal Quest _quest;
        
        public abstract void OnQuestStateChange(QuestState newState, QuestState oldState);
        public abstract void OnSetup();

        internal virtual void Complete()
        {
            if (completed) return;
            completed = true;
            _quest.CompleteAction(this);
        }
    }
}