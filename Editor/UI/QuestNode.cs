using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    public class QuestNode : Node
    {
        private Quest _quest;
        private InnerGraphView innerGraph;

        public Quest quest => _quest;

        public QuestNode(Quest quest)
        {
            _quest = quest;
            title = "no title";
            capabilities |= Capabilities.Renamable;

            Port port;
            outputContainer.Add(port = Port.Create<Edge>(Orientation.Horizontal,Direction.Output,Port.Capacity.Multi,null));
            port.AddManipulator(port.edgeConnector);
            inputContainer.Add(port = Port.Create<Edge>(Orientation.Horizontal,Direction.Input,Port.Capacity.Multi,null));
            port.AddManipulator(port.edgeConnector);


            var contents = this.Q("contents");
            
            if (quest is GroupQuest)
            {
                //Add children container
                var childrenContainer = new QuestGroupContent();
                childrenContainer.name = "childrenContainer";
                childrenContainer.AddToClassList("questGroupContainer");
                childrenContainer.RemoveFromClassList("graphElement");
                contents.Add(childrenContainer);
                
                //Inner GraphView
                innerGraph = new InnerGraphView();
                innerGraph.name = "innerGraphView";
                childrenContainer.Add(innerGraph);
                
                innerGraph.AddManipulator(new ContentDragger());
                innerGraph.AddManipulator(new SelectionDragger(){clampToParentEdges = true});
                innerGraph.AddManipulator(new RectangleSelector());
                innerGraph.AddManipulator(new ClickSelector());
                
                //Add Resizer
                childrenContainer.capabilities |= Capabilities.Resizable;
                childrenContainer.Add(new Resizer());
                
                //Add new button
                var newButton = new Button(AddGroupQuest);
                newButton.text = "+";
                newButton.AddToClassList("newButton");
                contents.Add(newButton);
            }
        }

        private Graph graph
        {
            get
            {
                var parent = this.parent;
                while (parent != null && !(parent is Graph))
                    parent = parent.parent;
                return parent as Graph;
            }
        }

        private void AddGroupQuest()
        {
            var graph = this.graph;
            var node = new QuestNode(new GroupQuest());
            graph.AddElement(node);
            innerGraph.Q("contentViewContainer").Add(node);
            //innerGraph.AddElement(node);
        }

        private class InnerGraphView : GraphView
        {
            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            {
                var parent = this.parent;
                while (parent != null && !(parent is Graph))
                    parent = parent.parent;
                var graph = parent as Graph;
                return graph?.GetCompatiblePorts(startPort, nodeAdapter);
            }
        }

        private class QuestGroupContent : GraphElement {}
    }
}