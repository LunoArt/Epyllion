using System;
using System.Collections.Generic;
using Luno.Epyllion.Actions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Luno.Epyllion.Editor.UI
{
    public class QuestNode : Node
    {

        internal Quest _quest;
        private readonly TextField _titleField;
        public readonly Port input;
        public readonly Port output;
        
        internal QuestNode(Quest quest)
        {
            _quest = quest;
            base.SetPosition(new Rect(quest.graphPosition,Vector2.zero));
            base.title = quest.name;
            //capabilities |= Capabilities.Resizable;
            
            //title editor
            _titleField = new TextField();
            _titleField.AddToClassList("titleField");
            _titleField.value = base.title;
            _titleField.RegisterCallback<InputEvent>(evt =>
            {
                quest.name = evt.newData;
                title = quest.name;
            });
            _titleField.RegisterCallback<FocusOutEvent>(evt => StopEditingTitle());
            titleContainer.Add(_titleField);
            
            //play controls
            extensionContainer.Add(new IMGUIContainer(() =>
            {
                if (!EditorApplication.isPlaying)
                    return;
                    
                GUILayout.Label(quest.state.ToString());
                GUILayout.Label("requirements left: "+quest._requiredLeft);
                    
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Activate"))
                {
                    quest.Activate();
                }

                if (GUILayout.Button("Complete"))
                {
                    quest.Complete();
                }
                GUILayout.EndHorizontal();
            }));
            
            //quest field
            var serializedQuest = new SerializedObject(quest);
            var actions = serializedQuest.FindProperty("actions");
            var questContainer = new IMGUIContainer(() =>
            {
                serializedQuest.Update();
                for (var a = 0; a < actions.arraySize; a++)
                {
                    var action = actions.GetArrayElementAtIndex(a);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    var actionEditor = UnityEditor.Editor.CreateEditor(action.objectReferenceValue);
                    actionEditor.OnInspectorGUI();

                    EditorGUILayout.BeginHorizontal();
                    
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove Action"))
                    {
                        if (action.objectReferenceValue is QuestSceneActionWrapper wrapper)
                        {
                            EpyllionWindow.BeginSceneEdit(wrapper._sceneAsset);
                            EpyllionWindow.GetSceneManager().WrapperDeleted(wrapper);
                            EpyllionWindow.EndSceneEdit();
                        }
                        ArrayUtility.RemoveAt(ref quest.actions, a);
                        Object.DestroyImmediate(action.objectReferenceValue, true);
                        EditorUtility.SetDirty(quest);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();

            });
            extensionContainer.Add(questContainer);
            

            //Ports section
            outputContainer.Add(output = Port.Create<Edge>(Orientation.Horizontal,Direction.Output,Port.Capacity.Multi,null));
            output.AddManipulator(output.edgeConnector);
            output.name = "output";
            inputContainer.Add(input = Port.Create<Edge>(Orientation.Horizontal,Direction.Input,Port.Capacity.Multi,null));
            input.AddManipulator(input.edgeConnector);
            input.name = "input";
            
            //Actions Drag
            RegisterCallback(new EventCallback<DragUpdatedEvent>((evt) =>
            {
                MonoScript draggedScript = DragAndDrop.objectReferences[0] as MonoScript;
                if (draggedScript == null) return;
                DragAndDrop.visualMode = 
                    draggedScript.GetClass().IsSubclassOf(typeof(QuestAction)) &&
                    !draggedScript.GetClass().IsSubclassOf(typeof(QuestSceneActionWrapper)) &&
                    draggedScript.GetClass() != typeof(QuestSceneActionWrapper) &&
                    !draggedScript.GetClass().IsAbstract
                        ? 
                    DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
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
        
        internal void StartEditingTitle()
        {
            titleContainer.AddToClassList("editing");
        }

        internal void StopEditingTitle()
        {
            titleContainer.RemoveFromClassList("editing");
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _quest.graphPosition = newPos.position;
            EditorUtility.SetDirty(_quest);
        }
        
        public void AddNewAction(System.Type actionType)
        {
            //create action
            var action = ScriptableObject.CreateInstance(actionType) as QuestAction;
            if (action == null) return;

            var sceneAction = action as QuestSceneAction;
            if (sceneAction != null)
            {
                var actionWrapper = ScriptableObject.CreateInstance<QuestSceneActionWrapper>();
                actionWrapper._action = sceneAction;
                actionWrapper._sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
                if (actionWrapper._sceneAsset == null)
                {
                    EditorUtility.DisplayDialog("Can't add Action","You want to add a QuestSceneAction, the active scene must be saved first","Ok");
                    return;
                }
                sceneAction._quest = _quest;
                sceneAction._wrapper = actionWrapper;
                var manager = EpyllionWindow.GetSceneManager();
                manager._actions.Add(sceneAction);
                EditorUtility.SetDirty(manager);
                action = actionWrapper;
            }
            
            action.hideFlags = HideFlags.HideInHierarchy;
            action._quest = _quest;
            AssetDatabase.AddObjectToAsset(action, _quest);
            ArrayUtility.Add(ref _quest.actions, action);
            EditorUtility.SetDirty(_quest);
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        /*private QuestNodeData _nodeData;
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
                _actionsContainer?.Clear();
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
            if (graph.sceneManager != null)
            {
                extensionContainer.Add(new IMGUIContainer(() =>
                {
                    if (!EditorApplication.isPlaying)
                        return;
                    
                    GUILayout.Label(graph.sceneManager.story._quests[quest.id].state.ToString());
                    GUILayout.Label("requirements left: "+graph.sceneManager.story._quests[quest.id]._requiredLeft);
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Activate"))
                    {
                        graph.sceneManager.story._quests[quest.id].Activate();
                    }

                    if (GUILayout.Button("Complete"))
                    {
                        graph.sceneManager.story._quests[quest.id].Complete();
                    }
                    GUILayout.EndHorizontal();
                }));
                _actionsContainer = new VisualElement();
                var addActionButton = new Button(ShowAddActionPopup);
                addActionButton.text = "Add Action";
                extensionContainer.Add(_actionsContainer);
                extensionContainer.Add(addActionButton);
            }

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
        }*/
    }
}