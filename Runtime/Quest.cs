using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luno.Epyllion
{
    public abstract class Quest : ScriptableObject
    {
        //Editor graph data
        [SerializeField] internal Vector2 graphPosition;
        
        
        internal int _id;
        internal Story _story;
        [SerializeField] internal GroupQuest _parent;
        [SerializeField] internal Quest[] _requirements = new Quest[0];
        [SerializeField] internal Quest[] _dependents = new Quest[0];
        [SerializeField] internal QuestAction[] actions = new QuestAction[0];
        internal GroupQuest _closestExclusiveParent;
        internal uint _requiredLeft;
        
        private bool _exclusive;
        public bool exclusive
        {
            get => _exclusive;
            set => _exclusive = value;
        }

        internal QuestState _state = QuestState.Blocked;
        public QuestState state
        {
            get => _state;
            protected set
            {
                if(_stateModificationBlock)
                    throw new Exception("You can't change the state of a Quest in an OnStateChanged event");
                if (_state == value) return;
                QuestState prevState = _state;
                _state = value;
                
                OnStateChange(_state, prevState);
            }
        }

        private void OnStateChange(QuestState newState, QuestState prevState)
        {
            LockStateModification();
            foreach (var action in actions)
            {
                action.OnQuestStateChange(newState,prevState);
            }
            UnlockStateModification();
        }

        internal void CompleteAction(QuestAction action)
        {
            if(_stateModificationBlock)
                throw new Exception("You can't change the state of a Quest in an OnStateChanged event");
            
            foreach (var questAction in actions)
            {
                if (!questAction.completed)
                    return;
            }
            Complete();
        }

        #region State Change

        private bool ValidateStateChange(QuestState newState, params QuestState[] valids)
        {
            if (newState == _state) return false;
            if (valids.Any(valid => _state == valid))
            {
                return true;
            }
            
            throw new Exception($"The quest state can't be set to {newState}. Current state: {_state}");
        }
        
        protected internal virtual void Pause()
        {
            state = QuestState.Paused;
        }

        protected internal virtual void Exclude()
        {
            state = QuestState.Excluded;
        }

        protected internal virtual void Include()
        {
            if (_state == QuestState.Paused)
                state = QuestState.Active;
            else if (_state == QuestState.Excluded)
                state = QuestState.Available;
        }

        protected internal virtual void Unblock()
        {
            switch (_parent._state)
            {
                case QuestState.Active:
                case QuestState.Available:
                    state = QuestState.Available;
                    break;
                case QuestState.Excluded:
                case QuestState.Paused:
                    state = QuestState.Excluded;
                    break;
                case QuestState.Blocked:
                case QuestState.Completed:
                    throw new Exception($"Can't unblock the quest as the parent is {_parent._state}");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Activate()
        {
            if(!ValidateStateChange(QuestState.Active,QuestState.Available,QuestState.Paused))
                return;
            
            //set the state of this quest as Active
            state = QuestState.Active;
            
            GroupQuest parent = _parent;
            Quest current = this;
            while (parent != null)
            {
                if (_exclusive)
                {
                    //for all the quests, not parents or children, that are active, mark it as paused
                    foreach (var child in parent.children)
                    {
                        if(child == current)
                            continue;
                        
                        switch (child.state)
                        {
                            case QuestState.Active:
                                child.Pause();
                                break;
                            case QuestState.Available:
                                child.Exclude();
                                break;
                        }
                    }
                }

                //mark the parent as Active
                parent.state = QuestState.Active;
                
                //set next step
                current = parent;
                parent = parent._parent;
            }
        }

        //TODO: Stop method => will allow the user to quit a started quest

        public void Complete()
        {
            if(!ValidateStateChange(QuestState.Completed,QuestState.Active, QuestState.Available))
                return;
            /*if (_state == QuestState.Completed)
                return;*/

            state = QuestState.Completed;

            //re-include the paused and excluded quests
            if (_exclusive && _closestExclusiveParent != null)
            {
                foreach (var child in _closestExclusiveParent.children)
                {
                    child.Include();
                }
            }

            //upwards propagation
            GroupQuest parent = _parent;
            while (parent != null)
            {
                //if all children completed, mark it as completed
                if (--parent._childrenLeft <= 0)
                    parent.state = QuestState.Completed;
                else
                    break;
                
                //next step
                parent = parent._parent;
            }
            
            //dependents propagation
            foreach (var dependent in _dependents)
            {
                if (--dependent._requiredLeft <= 0)
                {
                    /*if (dependent._parent._state != QuestState.Available ||
                        dependent._parent._state == QuestState.Excluded)
                        dependent.state = dependent._parent._state;*/
                    if (dependent._parent._state != QuestState.Blocked)
                        dependent.Unblock();
                }
            }
        }
        

        #endregion

        #region Static

        private static bool _stateModificationBlock;

        internal static void LockStateModification()
        {
            _stateModificationBlock = true;
        }

        internal static void UnlockStateModification()
        {
            _stateModificationBlock = false;
        }
        
        #endregion
    }
}