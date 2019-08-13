using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public class GroupQuest : Quest
    {
        internal Quest[] children = new Quest[0];
        
        protected internal override void Pause()
        {
            base.Pause();
            foreach (var child in children)
            {
                if (child.state == QuestState.Active)
                    child.Pause();
                else if (child.state == QuestState.Available)
                    child.Exclude();
            }
        }

        protected internal override void Exclude()
        {
            base.Exclude();
            foreach (var child in children)
            {
                if (child.state == QuestState.Available)
                    child.Exclude();
            }
        }

        protected internal override void Include()
        {
            base.Include();
            foreach (var child in children)
            {
                child.Include();
            }
        }
    }
}