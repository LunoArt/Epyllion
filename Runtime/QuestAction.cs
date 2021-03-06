using System;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestAction : ScriptableObject
    {
        public bool completed { get; internal set; }

        [SerializeField] [HideInInspector] internal Quest _quest;

        public Quest quest => _quest;

        public abstract void OnQuestStateChange(QuestState newState, QuestState oldState);
        public abstract void OnSetup();

        public virtual void Complete(string result = null)
        {
            quest._result = result;
            if (completed) return;
            completed = true;
            _quest.CompleteAction(this);
        }
    }
}