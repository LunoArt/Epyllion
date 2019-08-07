using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    public class QuestNode : Node
    {
        private QuestNodeData _nodeData;

        public QuestNodeData nodeData => _nodeData;

        public readonly Port input;
        public readonly Port output;

        public QuestNode(QuestNodeData quest)
        {
            _nodeData = quest;
            base.SetPosition(quest.GetPosition());
            base.title = "no title";
            capabilities |= Capabilities.Renamable;

            outputContainer.Add(output = Port.Create<Edge>(Orientation.Horizontal,Direction.Output,Port.Capacity.Multi,null));
            output.AddManipulator(output.edgeConnector);
            inputContainer.Add(input = Port.Create<Edge>(Orientation.Horizontal,Direction.Input,Port.Capacity.Multi,null));
            input.AddManipulator(input.edgeConnector);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _nodeData.SetPosition(newPos);
        }
    }
}