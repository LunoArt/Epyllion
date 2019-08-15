using System;
using Luno.Epyllion;
using UnityEngine.Events;

namespace Luno.Epyllion.Actions
{
    [Serializable]
    public class SimpleEventsAction : QuestAction
    {
        public QuestEvent onSetup;
        public QuestEvent onActivate;
        public QuestEvent onStart;
        public QuestEvent onComplete;
        
        public override void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            throw new NotImplementedException();
        }

        public override void OnSetup()
        {
            throw new NotImplementedException();
        }
    }
    
    [Serializable]
    public class QuestEvent : UnityEvent {}
}