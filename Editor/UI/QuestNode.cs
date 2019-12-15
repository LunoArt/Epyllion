using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Luno.Epyllion.Editor.UI
{
    public class QuestNode : Node
    {

        private readonly TextField _titleField;
        private UnityEditor.Editor[] _actionEditors;
        private bool[] _actionHides;
        
        internal Quest _quest;
        
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

                EditorGUIUtility.labelWidth = 100;
                EditorGUIUtility.fieldWidth = 100;
                
                //check if the editors has the same size as the actions
                if (_actionEditors == null)
                {
                    _actionEditors = new UnityEditor.Editor[actions.arraySize];
                    _actionHides = new bool[actions.arraySize];
                }else if (_actionEditors.Length != actions.arraySize)
                {
                    Array.Resize(ref _actionEditors, actions.arraySize);
                    Array.Resize(ref _actionHides, actions.arraySize);
                }
                
                for (var a = 0; a < actions.arraySize; a++)
                {
                    var action = (QuestAction) actions.GetArrayElementAtIndex(a).objectReferenceValue;
                    var wrapper = action as QuestSceneActionWrapper;
                    
                    EditorGUILayout.Space();
                    
                    //Action Editor...
                    if (_actionEditors[a] == null || _actionEditors[a].target != action)
                    {
                        _actionEditors[a] = UnityEditor.Editor.CreateEditor(action);
                    }

                    _actionHides[a] = !EditorGUILayout.Foldout(!_actionHides[a], (wrapper != null)? wrapper._actionType.name : action.GetType().Name);

                    if (!_actionHides[a])
                    {
                        EditorGUILayout.BeginVertical();

                        _actionEditors[a].OnInspectorGUI();

                        EditorGUILayout.BeginHorizontal();

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove Action"))
                        {
                            if (wrapper != null)
                            {
                                EpyllionWindow.BeginSceneEdit(wrapper.sceneAsset);
                                EpyllionWindow.GetSceneManager().WrapperDeleted(wrapper);
                                EpyllionWindow.EndSceneEdit();
                            }
                            ArrayUtility.RemoveAt(ref quest.actions, a);
                            Object.DestroyImmediate(action, true);
                            EditorUtility.SetDirty(quest);
                            break;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
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
                var actionWrapper = (QuestSceneActionWrapper) ScriptableObject.CreateInstance(typeof(QuestSceneActionWrapper));
                actionWrapper._action = sceneAction;
                actionWrapper._sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
                actionWrapper._actionType = MonoScript.FromScriptableObject(sceneAction);
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
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (action) => StartEditingTitle());
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }
    }
}
