using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    internal sealed class Graph : GraphView
    {
        public class GraphFactory : UnityEngine.UIElements.UxmlFactory<Graph> {}

        private QuestNode _quest;

        public Quest quest
        {
            get => _quest.quest;
            set
            {
                if (_quest != null)
                {
                    RemoveElement(_quest);
                    _quest = null;
                }

                if (value == null)
                    return;
                
                _quest = new QuestNode(value);
                AddElement(_quest);
                _quest.AddToClassList("rootQuest");
                _quest.style.position = Position.Relative;
                _quest.Q<GraphElement>("childrenContainer").capabilities &= ~Capabilities.Resizable;
                var innerGraph = _quest.Q<GraphView>("innerGraphView");
                innerGraph.SetupZoom(0.5f, 1);
            }
        }

        public Graph()
        {
            //SetupZoom(0.5f,1);
            //this.AddManipulator(new ContentDragger());
            //this.AddManipulator(new SelectionDragger());
            //this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            Debug.Log(ports.ToList().Count);
            return ports.ToList();
        }
    }
}