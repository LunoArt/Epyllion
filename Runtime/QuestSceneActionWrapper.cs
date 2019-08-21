using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    internal class QuestSceneActionWrapper : QuestAction
    {
        [SerializeField] internal SceneAsset _sceneAsset;
        [SerializeField] internal QuestSceneAction _action;

        internal bool initialized;
        
        public override void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if(_action != null)
                _action.OnQuestStateChange(newState,oldState);
        }

        public override void OnSetup()
        {
            if(_action != null)
                _action.OnSetup();
        }

        public void Initialize()
        {
            if(_quest._story.initialized)
                OnSetup();
            initialized = true;
        }
    }
}