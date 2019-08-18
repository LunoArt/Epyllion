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
            var quest = story.CreateQuest<TaskQuest>();
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
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        /*

        private void RebuildGraph()
        {
            _RebuildGraph();
            return;
            
            RemoveAllElements();
            
            if (story == null)
            {
                AddToClassList("empty");
                return;
            }
            RemoveFromClassList("empty");
            
            if (story.nodesData != null)
            {
                Dictionary<int, QuestNode> nodesDictionary = new Dictionary<int, QuestNode>();

                //load quest nodes
                for (var q = 0; q < story.nodesData.Length; q++)
                {
                    var quest = story.nodesData[q];
                    QuestNode node = new QuestNode(this, quest, -1);
                    
                    nodesDictionary.Add(quest.id,node);
                    
                    AddElement(node);
                }

                //load connections
                foreach (var quest in story.nodesData)
                {
                    if (quest.requirements != null)
                    {
                        foreach (var requirement in quest.requirements)
                        {
                            AddElement(nodesDictionary[requirement].output.ConnectTo(nodesDictionary[quest.id].input));
                        }
                    }
                }
                
                //load the binders
                if (sceneManager != null)
                {
                    for (var i = 0; i < sceneManager.binders.Length; i++)
                    {
                        nodesDictionary[sceneManager.binders[i].id].binderIndex = i;
                    }
                }
            }
        }

        private GraphViewChange OnChange(GraphViewChange changes)
        {
            var hasToRebuild = false;
            if (changes.elementsToRemove != null)
            {
                foreach (var edge in changes.elementsToRemove.OfType<Edge>())
                {
                    (edge.input.node as QuestNode).nodeData.RemoveRequirement((edge.output.node as QuestNode).nodeData.id);
                }

                foreach (var node in changes.elementsToRemove.OfType<QuestNode>())
                {
                    story.RemoveNode(node.nodeData);
                    //remove the binder
                    if (sceneManager)
                    {
                        var serializedSceneManager = new SerializedObject(sceneManager);
                        var binderCount = serializedSceneManager.FindProperty("binders.Array.size").intValue;
                        for (int b = 0; b < binderCount; b++)
                        {
                            var id = serializedSceneManager.FindProperty("binders.Array.data[" + b + "].id").intValue;
                            if (id == node.nodeData.id)
                            {
                                serializedSceneManager.FindProperty("binders").DeleteArrayElementAtIndex(b);
                                serializedSceneManager.ApplyModifiedPropertiesWithoutUndo();
                                break;
                            }
                        }
                        EditorUtility.SetDirty(sceneManager);
                    }

                    hasToRebuild = true;
                }
            }

            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    (edge.input.node as QuestNode).nodeData.AddRequirement((edge.output.node as QuestNode).nodeData.id);
                }
            }
            
            EditorUtility.SetDirty(story);
            
            if(hasToRebuild)
                RebuildGraph();
            
            return changes;
        }


        public void SetTargets(Story structure, StorySceneManager manager)
        {
            story = structure;
            sceneManager = manager;
            RebuildGraph();
        }
        
        public void CreateNode(DropdownMenuAction action)
        {
            var quest = story.CreateNode<TaskQuest>();
            quest.title = "New Quest";
            var node = new QuestNode(this, quest, -1);
            AddElement(node);
            
            /*var sceneManagerSerialized = new SerializedObject(sceneManager);
            int i = sceneManagerSerialized.FindProperty("binders").arraySize++;
            sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "].id").intValue = quest.id;
            sceneManagerSerialized.ApplyModifiedPropertiesWithoutUndo();
            var serializedField = sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "]");
            
            PropertyField field = new PropertyField(serializedField);
            field.Bind(serializedField.serializedObject);
            node.SetContent(field);*/
            /*
            EditorUtility.SetDirty(story);
            
            node.StartEditingTitle();
        }

        public void RemoveAllElements()
        {
            graphElements.ForEach(RemoveElement);
        }
        
        /*public void Save(DropdownMenuAction action)
        {
            AssetDatabase.ForceReserializeAssets(new string[] {AssetDatabase.GetAssetPath(_story)});
            AssetDatabase.Refresh();
        }*/
/*
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> portList = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (port.node == startPort.node) continue;
                if (port.name == startPort.name) continue;
                portList.Add(port);
            }
            return portList;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", CreateNode, (a) => (EditorApplication.isPlayingOrWillChangePlaymode?DropdownMenuAction.Status.Disabled:DropdownMenuAction.Status.Normal));
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
            //evt.menu.AppendSeparator();
            //evt.menu.AppendAction("Save Graph", Save, (a) => DropdownMenuAction.Status.Normal);
        }*/
    }
}