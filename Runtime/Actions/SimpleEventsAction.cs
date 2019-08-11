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
    }
    
    [Serializable]
    public class QuestEvent : UnityEvent {}
}