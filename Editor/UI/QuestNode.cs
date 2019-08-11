using System;
using System.Collections.Generic;
using Luno.Epyllion.Actions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    public class QuestNode : Node
    {
        private QuestNodeData _nodeData;
        private TextField _titleField;
        private VisualElement _actionsContainer;
        private Graph _graph;
        private PropertyField field;

        private int _binderIndex;
        public int binderIndex
        {
            get { return _binderIndex; }
            internal set
            {
                _binderIndex = value;
                _actionsContainer.Clear();
                if(_binderIndex < 0) return;
                
                var sceneManagerSerialized = new SerializedObject(_graph.sceneManager);
                var serializedField = sceneManagerSerialized.FindProperty("binders.Array.data[" + _binderIndex + "]");
                field = new PropertyField(serializedField);
                field.BindProperty(serializedField);
                _actionsContainer.Add(field);
            }
        }

        public QuestNodeData nodeData => _nodeData;

        public readonly Port input;
        public readonly Port output;

        internal QuestNode(Graph graph, QuestNodeData quest, int binderIndex)
        {
            _graph = graph;
            _nodeData = quest;
            base.SetPosition(quest.GetPosition());
            base.title = quest.title;
            //capabilities |= Capabilities.Resizable;
            
            //title editor
            _titleField = new TextField();
            _titleField.AddToClassList("titleField");
            _titleField.value = base.title;
            _titleField.RegisterCallback<InputEvent>(evt =>
            {
                quest.title = evt.newData;
                title = quest.title;
            });
            _titleField.RegisterCallback<FocusOutEvent>(evt => StopEditingTitle());
            titleContainer.Add(_titleField);

            //Ports section
            outputContainer.Add(output = Port.Create<Edge>(Orientation.Horizontal,Direction.Output,Port.Capacity.Multi,null));
            output.AddManipulator(output.edgeConnector);
            output.name = "output";
            inputContainer.Add(input = Port.Create<Edge>(Orientation.Horizontal,Direction.Input,Port.Capacity.Multi,null));
            input.AddManipulator(input.edgeConnector);
            input.name = "input";
            
            //Actions section
            _actionsContainer = new VisualElement();
            var addActionButton = new Button(ShowAddActionPopup);
            addActionButton.text = "Add Action";
            extensionContainer.Add(_actionsContainer);
            extensionContainer.Add(addActionButton);
            
            this.binderIndex = binderIndex;
            
            //Actions Drag
            RegisterCallback(new EventCallback<DragUpdatedEvent>((evt) =>
            {
                //Debug.Log("update");
                MonoScript draggedScript = DragAndDrop.objectReferences[0] as MonoScript;
                if (draggedScript == null) return;
                if (draggedScript.GetClass().IsSubclassOf(typeof(QuestAction)))
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                else
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }));
            RegisterCallback(new EventCallback<DragPerformEvent>((evt) =>
            {
                MonoScript draggedScript = DragAndDrop.objectReferences[0] as MonoScript;
                if (draggedScript == null) return;
                DragAndDrop.AcceptDrag();
                AddNewAction(draggedScript.GetClass());
            }));
            
            RefreshExpandedState();
        }

        private void CreateBinder()
        {
            var sceneManagerSerialized = new SerializedObject(_graph.sceneManager);
            var index = sceneManagerSerialized.FindProperty("binders").arraySize++;
            sceneManagerSerialized.FindProperty("binders.Array.data[" + index + "].id").intValue = _nodeData.id;
            sceneManagerSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(_graph.sceneManager);

            binderIndex = index;
        }

        internal void StartEditingTitle()
        {
            titleContainer.AddToClassList("editing");
        }

        internal void StopEditingTitle()
        {
            titleContainer.RemoveFromClassList("editing");
        }

        public void ShowAddActionPopup()
        {
            EditorGUIUtility.ShowObjectPicker<MonoScript>(null,false,"",0);
            
        }

        public void AddNewAction(System.Type actionType)
        {
            if (binderIndex < 0)
            {
                CreateBinder();
            }
            
            //create action
            var action = ScriptableObject.CreateInstance(actionType) as QuestAction;
            ArrayUtility.Add(ref _graph.sceneManager.binders[binderIndex].actions, action);
            
            field.Bind(new SerializedObject(_graph.sceneManager));
            EditorUtility.SetDirty(_graph.sceneManager);
        }
        

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _nodeData.SetPosition(newPos);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (action) => StartEditingTitle());
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }
    }
}