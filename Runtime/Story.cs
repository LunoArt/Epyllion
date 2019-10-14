using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Luno.Epyllion
{
    public class Story : ScriptableObject
    {
        [SerializeField] private GroupQuest rootQuest;
        //[SerializeField] private bool initializeEmpty;
        [SerializeField] private int lastId;

        public event UnityAction<Quest, QuestState> OnQuestStateChange;
        
        public bool initialized { get; private set; }
        
        internal GroupQuest RootQuest
        {
            get
            {
                if (rootQuest == null)
                    rootQuest = CreateQuest<GroupQuest>();
                return rootQuest;
            }
        }

        internal void QuestStateChanged(Quest quest, QuestState previousState)
        {
            OnQuestStateChange?.Invoke(quest,previousState);
        }

#region Editor
#if UNITY_EDITOR
        internal T CreateQuest<T>() where T : Quest
        {
            var quest = CreateInstance<T>();
            quest.hideFlags = HideFlags.HideInHierarchy;
            quest._id = lastId++;
            quest._story = this;
            AssetDatabase.AddObjectToAsset(quest,this);
            return quest;
        }

        Story()
        {
            EditorApplication.playModeStateChanged += OnPlayModeState;
        }

        private void OnPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ForEach(rootQuest, quest =>
                {
                    foreach (var action in quest.actions)
                    {
                        action.completed = false;
                    }
                    quest._state = QuestState.Available;
                });
                initialized = false;
            }
        }
#endif
#endregion

        /*//initialize the story in runtime
        private void OnEnable()
        {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            #endif
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            if(initializeEmpty) SetState(CalculateInitialState());
        }*/

        public StoryState CalculateInitialState()
        {
            var stateDictionary = new Dictionary<int, QuestState>();
            ForEach(rootQuest, (quest) =>
            {
                var open = (quest._parent == null || quest._parent._state != QuestState.Blocked) && quest._requirements.Length == 0;
                stateDictionary.Add(quest._id, open?QuestState.Available : QuestState.Blocked);
            });
            
            
            var state = new StoryState(){Ids = stateDictionary.Keys.ToArray(), States = stateDictionary.Values.ToArray()};
            return state;
        }

        public void SetState(StoryState state)
        {
            var stateDictionary = new Dictionary<int,QuestState>();
            
            for (var i = 0; i < state.Ids.Length; i++)
            {
                stateDictionary.Add(state.Ids[i],state.States[i]);
            }
            
            ForEach(rootQuest, (quest) =>
            {
                quest._state = stateDictionary[quest._id];
            });
            
            ForEach(rootQuest, (quest) =>
            {
                var requiredLeft = (uint) quest._requirements.Length;
                foreach (var requirement in quest._requirements)
                {
                    if (requirement._state == QuestState.Completed)
                        requiredLeft--;
                }
                quest._requiredLeft = requiredLeft;

                if (!(quest is GroupQuest groupQuest)) return;
                
                var childrenLeft = (uint) groupQuest.children.Length;
                foreach (var child in groupQuest.children)
                {
                    if (child._state == QuestState.Completed)
                        childrenLeft--;
                }
                groupQuest._childrenLeft = childrenLeft;
            });
            
            //setup all the actions
            ForEach(rootQuest, quest =>
            {
                foreach (var action in quest.actions)
                {
                    action.OnSetup();
                }
            });

            initialized = true;
        }

        private static void ForEach(Quest quest, UnityAction<Quest> method)
        {
            method(quest);
            if (!(quest is GroupQuest groupQuest)) return;
            foreach (var child in groupQuest.children)
            {
                ForEach(child, method);
            }
        }
    }
}