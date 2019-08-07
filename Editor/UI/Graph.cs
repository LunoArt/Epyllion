using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    internal sealed class Graph : GraphView
    {
        public class GraphFactory : UnityEngine.UIElements.UxmlFactory<Graph> {}

        private StoryStructure _story;

        public StoryStructure story
        {
            get => _story;
            set
            {
                if (_story != null)
                {
                    RemoveAllElements();
                    _story = null;
                }

                _story = value;

                if (_story == null)
                {
                    AddToClassList("empty");
                    return;
                }
                RemoveFromClassList("empty");

                if (_story.quests != null)
                {
                    Dictionary<int, QuestNode> nodesDictionary = new Dictionary<int, QuestNode>();
                    foreach (var quest in _story.quests)
                    {
                        QuestNode node = new QuestNode(quest);
                        nodesDictionary.Add(quest.id,node);
                        AddElement(node);
                    }

                    foreach (var quest in _story.quests)
                    {
                        if (quest.requirements != null)
                        {
                            foreach (var requirement in quest.requirements)
                            {
                                AddElement(nodesDictionary[requirement].output.ConnectTo(nodesDictionary[quest.id].input));
                            }
                        }
                    }
                }



                /*_quest = new QuestNode(value);
                AddElement(_quest);
                _quest.AddToClassList("rootQuest");
                _quest.style.position = Position.Relative;
                _quest.Q<GraphElement>("childrenContainer").capabilities &= ~Capabilities.Resizable;
                var innerGraph = _quest.Q<GraphView>("innerGraphView");
                innerGraph.SetupZoom(0.5f, 1);*/
            }
        }

        public Graph()
        {
            //SetupZoom(0.5f,1);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            graphViewChanged = OnChange;
        }

        private GraphViewChange OnChange(GraphViewChange changes)
        {
            if (changes.elementsToRemove != null)
            {
                foreach (var edge in changes.elementsToRemove.OfType<Edge>())
                {
                    (edge.input.node as QuestNode).nodeData.RemoveRequirement((edge.output.node as QuestNode).nodeData.id);
                }

                foreach (var node in changes.elementsToRemove.OfType<QuestNode>())
                {
                    story.RemoveNode(node.nodeData);
                }
            }

            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    Debug.Log("creating edge");
                    (edge.input.node as QuestNode).nodeData.AddRequirement((edge.output.node as QuestNode).nodeData.id);
                }
            }
            
            return changes;
        }
        
        

        public void CreateNode(DropdownMenuAction action)
        {
            AddElement(new QuestNode(_story.CreateNode<TaskQuest>()));
        }

        public void RemoveAllElements()
        {
            graphElements.ForEach(RemoveElement);
        }
        
        public void Save(DropdownMenuAction action)
        {
            AssetDatabase.ForceReserializeAssets(new string[] {AssetDatabase.GetAssetPath(_story)});
            AssetDatabase.Refresh();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> portList = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;
                portList.Add(port);
            }
            return portList;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", CreateNode, (a) => DropdownMenuAction.Status.Normal);
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Save Graph", Save, (a) => DropdownMenuAction.Status.Normal);
        }
    }
}