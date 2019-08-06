using System;
using UnityEngine;

namespace Luno.Epyllion
{
    public class Story : MonoBehaviour
    {
        [SerializeField]
        private GroupQuest _quest;

        public GroupQuest quest => _quest;

        public Story()
        {
            _quest = new GroupQuest();
        }
    }
}