using UnityEngine;

namespace Luno.Epyllion.Actions
{
    public class DebugAction : QuestAction
    {
        public override void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            Debug.Log($"Quest '{_quest.name}' state change from {oldState} to {newState}");
        }

        public override void OnSetup()
        {
            Debug.Log($"Quest '{_quest.name}' setup with state {_quest.state}");
        }
    }
}