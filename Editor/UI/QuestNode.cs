using System.Collections.Generic;
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

        public QuestNodeData nodeData => _nodeData;

        public readonly Port input;
        public readonly Port output;

        internal QuestNode(QuestNodeData quest)
        {
            _nodeData = quest;
            base.SetPosition(quest.GetPosition());
            base.title = quest.title;
            
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

            outputContainer.Add(output = Port.Create<Edge>(Orientation.Horizontal,Direction.Output,Port.Capacity.Multi,null));
            output.AddManipulator(output.edgeConnector);
            output.name = "output";
            inputContainer.Add(input = Port.Create<Edge>(Orientation.Horizontal,Direction.Input,Port.Capacity.Multi,null));
            input.AddManipulator(input.edgeConnector);
            input.name = "input";
        }

        internal void StartEditingTitle()
        {
            titleContainer.AddToClassList("editing");
        }

        internal void StopEditingTitle()
        {
            titleContainer.RemoveFromClassList("editing");
        }

        internal void SetContent(VisualElement content)
        {
            extensionContainer.Add(content);
            RefreshExpandedState();
            /*if (nodeBinder != null)
            {
                /*var field = new PropertyField(serializedProperty, "field");
                field.Bind(serializedProperty.serializedObject);
                extensionContainer.Add(field);

                IMGUIContainer imguiContainer = new IMGUIContainer(() =>
                {
                    UnityEditor.Editor.CreateEditor(nodeBinder.obj);
                });
            }*/
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