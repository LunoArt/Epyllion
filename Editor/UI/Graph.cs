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

        public StoryStructure story { get; private set; }
        public StorySceneManager sceneManager { get; private set; }

        public Graph()
        {
            SetupZoom(0.5f,1);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            graphViewChanged = OnChange;
        }

        private class BindElement
        {
            public QuestNode node;
            public int binderIndex;
            public SerializedProperty binderField;
        }

        private void RebuildGraph()
        {
            RemoveAllElements();
            
            if (story == null)
            {
                AddToClassList("empty");
                return;
            }
            RemoveFromClassList("empty");
            
            if (story.quests != null)
            {
                Dictionary<int, BindElement> nodesDictionary = new Dictionary<int, BindElement>();

                SerializedObject sceneManagerSerialized = null;
                //load the binders
                if (sceneManager != null)
                {
                    sceneManagerSerialized = new SerializedObject(sceneManager);
                    for (var i = 0; i < sceneManager.binders.Length; i++)
                    {
                        nodesDictionary.Add(sceneManager.binders[i].id, new BindElement()
                        {
                            binderIndex = i,
                            binderField = sceneManagerSerialized.FindProperty("binders.Array.data["+i+"]")
                        });
                    }
                }


                for (var q = 0; q < story.quests.Length; q++)
                {
                    var quest = story.quests[q];
                    QuestNode node = new QuestNode(quest);
                    
                    //bind to the scene manager
                    if (!nodesDictionary.TryGetValue(quest.id, out var bindElement))
                    {
                        bindElement = new BindElement();
                        if (sceneManagerSerialized != null)
                        {
                            int i = sceneManagerSerialized.FindProperty("binders").arraySize++;
                            sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "].id").intValue = quest.id;
                            sceneManagerSerialized.ApplyModifiedPropertiesWithoutUndo();
                            bindElement.binderIndex = i;
                            bindElement.binderField =
                                sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "]");
                        }
                        nodesDictionary.Add(quest.id,bindElement);
                    }
                    bindElement.node = node;

                    if (sceneManagerSerialized != null)
                    {
                        PropertyField field = new PropertyField(bindElement.binderField);
                        field.Bind(bindElement.binderField.serializedObject);
                        node.SetContent(field);
                    }
                    
                    AddElement(node);
                }

                foreach (var quest in story.quests)
                {
                    if (quest.requirements != null)
                    {
                        foreach (var requirement in quest.requirements)
                        {
                            AddElement(nodesDictionary[requirement].node.output.ConnectTo(nodesDictionary[quest.id].node.input));
                        }
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


        public void SetTargets(StoryStructure structure, StorySceneManager manager)
        {
            story = structure;
            sceneManager = manager;
            RebuildGraph();
        }
        
        public void CreateNode(DropdownMenuAction action)
        {
            var quest = story.CreateNode<TaskQuest>();
            quest.title = "New Quest";
            var node = new QuestNode(quest);
            AddElement(node);
            
            var sceneManagerSerialized = new SerializedObject(sceneManager);
            int i = sceneManagerSerialized.FindProperty("binders").arraySize++;
            sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "].id").intValue = quest.id;
            sceneManagerSerialized.ApplyModifiedPropertiesWithoutUndo();
            var serializedField = sceneManagerSerialized.FindProperty("binders.Array.data[" + i + "]");
            
            PropertyField field = new PropertyField(serializedField);
            field.Bind(serializedField.serializedObject);
            node.SetContent(field);
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
            evt.menu.AppendAction("Create Node", CreateNode, (a) => DropdownMenuAction.Status.Normal);
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
            //evt.menu.AppendSeparator();
            //evt.menu.AppendAction("Save Graph", Save, (a) => DropdownMenuAction.Status.Normal);
        }
    }
}