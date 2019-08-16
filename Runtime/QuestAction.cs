using System;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class QuestAction : ScriptableObject
    {
        public abstract void OnQuestStateChange(QuestState newState, QuestState oldState);
        public abstract void OnSetup();

        public void Complete()
        {
            
        }
    }
}