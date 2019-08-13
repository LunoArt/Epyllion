using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Luno.Epyllion
{
    public class Story : ScriptableObject, ISerializationCallbackReceiver
    {
        private int _lastId;
        private List<StorySceneManager> _managers = new List<StorySceneManager>();
        private Dictionary<int, Quest> _quests;

        [SerializeField]
        internal QuestNodeData[] nodesData = new QuestNodeData[0];
        
        

        //initialize the story in runtime
        private void OnEnable()
        {
            _quests = new Dictionary<int, Quest>();
            
            //root quest
            _quests.Add(0, new GroupQuest());
            
            //load all nodes
            foreach (var node in nodesData)
            {
                var quest = new TaskQuest();
                quest.exclusive = node.exclusive;
                _quests.Add(node.id, quest);
            }
            
            //set dependencies
            foreach (var node in nodesData)
            {
                var quest = _quests[node.id];
                
                //parent
                quest._parent = _quests[node.parent] as GroupQuest;
                if (quest._parent == null)
                {
                    throw new Exception("Parent node must be a GroupQuest");
                }
                ArrayUtility.Add(ref quest._parent.children, quest);
                if (quest._parent.exclusive)
                {
                    SetClosestExclusiveParent(quest._parent.children,quest._parent);
                }
                else if(quest._parent._closestExclusiveParent != null)
                {
                    SetClosestExclusiveParent(new []{quest}, quest._parent._closestExclusiveParent);
                }

                //requirements
                foreach (var requirementId in node.requirements)
                {
                    var requirement = _quests[requirementId];
                    ArrayUtility.Add(ref requirement._dependents, quest);
                    ArrayUtility.Add(ref quest._requirements, requirement);
                }
            }
        }

        private void SetClosestExclusiveParent(Quest[] quests, GroupQuest exclusiveParent)
        {
            foreach (var quest in quests)
            {
                quest._closestExclusiveParent = exclusiveParent;
                if (!quest.exclusive && quest is GroupQuest groupQuest) SetClosestExclusiveParent(groupQuest.children, exclusiveParent);
            }            
        }

        public QuestNodeData CreateNode<T>() where T : Quest
        {
            QuestNodeData node = new QuestNodeData() {id = ++_lastId};
            ArrayUtility.Add(ref nodesData, node);
            return node;
        }

        public void RemoveNode(QuestNodeData node)
        {
            ArrayUtility.Remove(ref nodesData, node);
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            foreach (var quest in nodesData)
            {
                _lastId = Mathf.Max(_lastId, quest.id);
            }
        }

        public void RegisterManager(StorySceneManager manager)
        {
            _managers.Add(manager);
        }

        public void UnregisterManager(StorySceneManager manager)
        {
            _managers.Remove(manager);
        }

        public void SetState(StoryState getInitialState)
        {
            throw new NotImplementedException();
        }

        public StoryState GetInitialState()
        {
            throw new NotImplementedException();
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