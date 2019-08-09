using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Luno.Epyllion
{
    [Serializable]
    public class StoryStructure : ScriptableObject, ISerializationCallbackReceiver
    {
        private int lastId;
        public QuestNodeData[] quests = new QuestNodeData[0];

        public QuestNodeData CreateNode<T>() where T : Quest
        {
            QuestNodeData node = new QuestNodeData() {id = ++lastId};
            ArrayUtility.Add(ref quests, node);
            return node;
        }

        public void RemoveNode(QuestNodeData node)
        {
            ArrayUtility.Remove(ref quests, node);
        }
        
        public void OnBeforeSerialize()
        {
            //throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
            foreach (var quest in quests)
            {
                lastId = Mathf.Max(lastId, quest.id);
            }
        }
    }

    [Serializable]
    public class QuestNodeData
    {
        public int id;
        public string title;
        public int parent = 0;
        public int[] requirements = new int[0];
        public string position = "";

        public Rect GetPosition()
        {
            string[] coords = position.Split(',');
            if (coords.Length != 2)
            {
                return Rect.zero;
            }
            return new Rect(new Vector2(int.Parse(coords[0]),int.Parse(coords[1])), Vector2.zero);
        }

        public void SetPosition(Rect position)
        {
            this.position = (int)position.x+","+(int)position.y;
        }

        public void AddRequirement(int id)
        {
            if (ArrayUtility.IndexOf(requirements, id) != -1)
                return;
            ArrayUtility.Add(ref requirements, id);
        }

        public void RemoveRequirement(int id)
        {
            ArrayUtility.Remove(ref requirements, id);
        }
    }
}