using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Luno.Epyllion.Editor.UI
{
    internal sealed class Graph : GraphView
    {
        public class GraphFactory : UnityEngine.UIElements.UxmlFactory<Graph> {}

        private Dictionary<Quest, QuestNode> nodesDictionary = new Dictionary<Quest, QuestNode>();
        private Story _story;
        public Story story {
            get => _story;
            set
            {
                _story = value;
                RebuildGraph();
            }
        }
        //public StorySceneManager sceneManager { get; private set; }

        public Graph()
        {
            SetupZoom(0.5f,1);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            graphViewChanged = OnChange;
        }

        private void RebuildGraph()
        {
            nodesDictionary.Clear();
            RemoveAllElements();
            
            if (story == null)
            {
                AddToClassList("empty");
                return;
            }
            RemoveFromClassList("empty");
            
            //
            BuildChildrenNodes(story.RootQuest);
            BuildConnections(story.RootQuest);
        }
        
        private void RemoveAllElements()
        {
            graphElements.ForEach(RemoveElement);
        }

        private void BuildChildrenNodes(GroupQuest groupQuest)
        {
            foreach (var child in groupQuest.children)
            {
                BuildNode(child);
            }
        }

        private QuestNode BuildNode(Quest quest)
        {
            var questNode = new QuestNode(quest);
            AddElement(questNode);
            nodesDictionary.Add(quest,questNode);
            return questNode;
        }

        private void BuildConnections(Quest quest)
        {
            foreach (var requirement in quest._requirements)
                AddElement(nodesDictionary[requirement].output.ConnectTo(nodesDictionary[quest].input));

            if (!(quest is GroupQuest groupQuest)) return;
            foreach (var child in groupQuest.children)
                BuildConnections(child);
        }
        
        public void CreateNode(DropdownMenuAction action)
        {
            var quest = story.CreateQuest<Quest>();
            quest.name = "New Quest";
            quest._parent = story.RootQuest;
            ArrayUtility.Add(ref story.RootQuest.children, quest);
            BuildNode(quest).StartEditingTitle();
            EditorUtility.SetDirty(story);
        }
        
        private GraphViewChange OnChange(GraphViewChange changes)
        {
            var hasToRebuild = false;
            if (changes.elementsToRemove != null)
            {
                foreach (var edge in changes.elementsToRemove.OfType<Edge>())
                {
                    var requirementQuest = (edge.output.node as QuestNode)?._quest;
                    var dependentQuest = (edge.input.node as QuestNode)?._quest;
                    
                    if (dependentQuest == null || requirementQuest == null) break;
                    
                    ArrayUtility.Remove(ref dependentQuest._requirements, requirementQuest);
                    ArrayUtility.Remove(ref requirementQuest._dependents, dependentQuest);
                }

                foreach (var node in changes.elementsToRemove.OfType<QuestNode>())
                {
                    var scenesToRemoveFrom = new HashSet<SceneAsset>();
                    foreach (var action in node._quest.actions)
                    {
                        if (action is QuestSceneActionWrapper wrapper)
                            scenesToRemoveFrom.Add(wrapper.sceneAsset);
                    }
                    foreach (var scene in scenesToRemoveFrom)
                    {
                        EpyllionWindow.BeginSceneEdit(scene);
                        EpyllionWindow.GetSceneManager().QuestDeleted(node._quest);
                        EpyllionWindow.EndSceneEdit();
                    }
                    ArrayUtility.Remove(ref node._quest._parent.children, node._quest);
                    Object.DestroyImmediate(node._quest,true);
                }
            }

            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    var requirementQuest = (edge.output.node as QuestNode)?._quest;
                    var dependentQuest = (edge.input.node as QuestNode)?._quest;
                    
                    if (dependentQuest == null || requirementQuest == null) break;
                    
                    ArrayUtility.Add(ref dependentQuest._requirements, requirementQuest);
                    ArrayUtility.Add(ref requirementQuest._dependents, dependentQuest);
                }
            }
            
            EditorUtility.SetDirty(story);
            
            if(hasToRebuild)
                RebuildGraph();
            
            return changes;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", CreateNode, (a) => DropdownMenuAction.Status.Normal);
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var portList = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;
                if (port.name == startPort.name) continue;
                portList.Add(port);
            }
            return portList;
        }
    }
}