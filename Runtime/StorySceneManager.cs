using System;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

namespace Luno.Epyllion
{
    public class StorySceneManager: MonoBehaviour
    {
        #pragma warning disable 649
        [SerializeField] private Story _story;
        #pragma warning restore 649
        public Story story => _story;
        [SerializeField] internal NodeBinder[] binders = new NodeBinder[0];

        private Dictionary<int, NodeBinder> binderDictionary;

        private void OnEnable()
        {
            if (story == null) return;
            binderDictionary = new Dictionary<int, NodeBinder>();
            Quest.LockStateModification();
            foreach (var binder in binders)
            {
                binderDictionary.Add(binder.id,binder);
                foreach (var action in binder.actions)
                {
                    action.OnSetup();
                }
            }
            Quest.UnlockStateModification();
            story.RegisterManager(this);
        }

        private void OnDisable()
        {
            if (story == null) return;
            story.UnregisterManager(this);
        }

        internal void OnQuestStateChange(Quest quest, QuestState prevState)
        {
            binderDictionary.TryGetValue(quest._id, out var binder);
            if (binder == null) return;
            foreach (var action in binder.actions)
            {
                action.OnQuestStateChange(quest.state,prevState);
            }
        }
    }
    
    [Serializable]
    internal class NodeBinder
    {
        public int id;
        public QuestAction[] actions = new QuestAction[0];
    }    
}