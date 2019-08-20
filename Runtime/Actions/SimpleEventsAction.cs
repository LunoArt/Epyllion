using System;
using Luno.Epyllion;
using UnityEngine;
using UnityEngine.Events;

namespace Luno.Epyllion.Actions
{
    [Serializable]
    public class SimpleEventsAction : QuestSceneAction
    {
        public QuestEvent onSetup;
        public QuestEvent onActivate;
        public QuestEvent onStart;
        public QuestEvent onComplete;
        
        public override void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            Debug.Log($"Scene Action: Quest '{_quest.name}' state change from {oldState} to {newState}");
        }

        public override void OnSetup()
        {
            Debug.Log($"Scene Action: Quest '{_quest.name}' setup with state {_quest.state}");
        }
    }
    
    [Serializable]
    public class QuestEvent : UnityEvent {}
}