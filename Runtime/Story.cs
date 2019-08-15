using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Luno.Epyllion
{
    public class Story : ScriptableObject, ISerializationCallbackReceiver
    {
        private int _lastId;
        private List<StorySceneManager> _managers = new List<StorySceneManager>();
        internal Dictionary<int, Quest> _quests = new Dictionary<int, Quest>();

        [SerializeField]
        internal QuestNodeData[] nodesData = new QuestNodeData[0];
        [SerializeField]
        private bool initializeEmpty;


        #region initialization
        //initialize the story in runtime
        private void OnEnable()
        {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            #endif
            
            //root quest
            _quests.Add(0, new GroupQuest {_story = this, _state = QuestState.Blocked});
            
            //load all nodes
            foreach (var node in nodesData)
            {
                var quest = new TaskQuest {_story = this, _id = node.id, exclusive = node.exclusive, _state = QuestState.Blocked};
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
                    SetClosestExclusiveParent(new []{quest},quest._parent);
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
            if(initializeEmpty) SetState(CalculateInitialState());
        }

        private void SetClosestExclusiveParent(Quest[] quests, GroupQuest exclusiveParent)
        {
            foreach (var quest in quests)
            {
                quest._closestExclusiveParent = exclusiveParent;
                if (!quest.exclusive && quest is GroupQuest groupQuest) SetClosestExclusiveParent(groupQuest.children, exclusiveParent);
            }            
        }
        #endregion

        private void OnDisable()
        {
            _managers.Clear();
            _quests.Clear();
        }

        public StoryState CalculateInitialState()
        {
            var ids = new int[_quests.Count];
            var states = new QuestState[_quests.Count];
            var count = -1;
            ComputeInitialState(_quests[0],true, ref ids, ref states, ref count);

            states[0] = QuestState.Available;
            
            var state = new StoryState(){Ids = ids, States = states};
            return state;
        }

        private void ComputeInitialState(Quest quest, bool open , ref int[] ids, ref QuestState[] states, ref int count)
        {
            count++;
            open &= quest._requirements.Length == 0;
            ids[count] = quest._id;
            states[count] = open ? QuestState.Available : QuestState.Blocked;

            if(quest is GroupQuest groupQuest)
            {
                foreach (var child in groupQuest.children)
                {
                    ComputeInitialState(child, open, ref ids, ref states, ref count);
                }
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

        public void SetState(StoryState state)
        {
            var indexes = new Dictionary<int,int>();
            
            for (var i = 0; i < state.Ids.Length; i++)
            {
                var id = state.Ids[i];
                indexes.Add(id, i);
            }

            for (var i = 0; i < state.Ids.Length; i++)
            {
                var id = state.Ids[i];
                _quests[id]._state = state.States[i];
                
                //requirements left
                uint requirementsLeft = 0;
                foreach (var requirement in _quests[id]._requirements)
                {
                    if (state.States[indexes[requirement._id]] != QuestState.Completed)
                        requirementsLeft++;
                }
                _quests[id]._requiredLeft = requirementsLeft;

                //children left
                if (_quests[id] is GroupQuest groupQuest)
                {
                    uint childrenLeft = 0;
                    foreach (var child in groupQuest.children)
                    {
                        if (state.States[indexes[child._id]] != QuestState.Completed)
                            childrenLeft++;
                    }

                    groupQuest._childrenLeft = childrenLeft;
                }
            }
        }

        internal void QuestStateChange(Quest quest, QuestState prevState)
        {
            foreach (var manager in _managers)
            {
                manager.OnQuestStateChange(quest, prevState);
            }
        }
    }
}