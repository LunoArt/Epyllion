using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luno.Epyllion
{
    [Serializable]
    public class Quest
    {
        [SerializeField] private GroupQuest _parent;
        [SerializeField] private Quest[] _requireds;

        private GroupQuest _closestExclusiveParent;
        private Quest[] _dependents;
        private uint _requiredLeft;
        private uint _childrenLeft;

        public delegate void StateChangeDelegate(Quest quest, QuestState prevState);
        public event StateChangeDelegate OnStateChanged;
        
        private bool _exclusive;
        public bool exclusive
        {
            get => _exclusive;
            set => _exclusive = value;
        }

        private QuestState _state;
        public QuestState state
        {
            get => _state;
            private set
            {
                if(_stateModificationBlock)
                    throw new Exception("You can't change the state of a Quest in an OnStateChanged event");
                if (_state == value) return;
                QuestState prevState = _state;
                _state = value;
                LockStateModification();
                OnStateChanged?.Invoke(this,prevState);
                UnlockStateModification();
            }
        }

        #region State Change

        private bool ValidateStateChange(QuestState newState, params QuestState[] valids)
        {
            if (newState == _state) return false;
            if (valids.Any(valid => _state == valid))
            {
                return true;
            }
            
            throw new Exception(String.Format("The quest state can't be set to %s. Current state: %s",newState,_state));
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
            if (_state == QuestState.Completed)
                return;

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
                //if all children completed, mark is as completed
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
                    if (dependent._parent._state != QuestState.Available ||
                        dependent._parent._state == QuestState.Excluded)
                        dependent.state = dependent._parent._state;
                }
            }
        }
        

        #endregion

        #region Static

        private static bool _stateModificationBlock;

        private static void LockStateModification()
        {
            _stateModificationBlock = true;
        }

        private static void UnlockStateModification()
        {
            _stateModificationBlock = false;
        }
        
        #endregion
    }
}