using System;
using UnityEditor;
using UnityEngine;

namespace Luno.Epyllion
{
    [Serializable]
    public class QuestNodeData
    {
        public int id;
        public string title;
        public int parent = 0;
        public int[] requirements = new int[0];
        public string position = "";
        public bool exclusive = false;

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