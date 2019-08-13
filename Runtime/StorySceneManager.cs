using System;
using UnityEngine;

namespace Luno.Epyllion
{
    public class StorySceneManager: MonoBehaviour
    {
        [SerializeField] private Story _story;
        public Story story => _story;
        [SerializeField] internal NodeBinder[] binders = new NodeBinder[0];


        private void OnEnable()
        {
            if (story == null) return;
            story.RegisterManager(this);
        }

        private void OnDisable()
        {
            if (story == null) return;
            story.UnregisterManager(this);
        }
    }
    
    [Serializable]
    internal class NodeBinder
    {
        public int id;
        public QuestAction[] actions = new QuestAction[0];
    }    
}