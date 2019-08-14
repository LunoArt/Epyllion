using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    public class GroupQuest : Quest
    {
        internal Quest[] children = new Quest[0];
        internal uint _childrenLeft;
        
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

        protected internal override void Unblock()
        {
            base.Unblock();
            foreach (var child in children)
            {
                child.Unblock();
            }
        }
    }
}