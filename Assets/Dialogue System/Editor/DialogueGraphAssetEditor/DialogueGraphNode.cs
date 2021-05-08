using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using CustomSystem.DialogueSystem;

namespace CustomEditors.DialogueSystem
{
    public class DialogueGraphNode : GraphNode
    {
        public struct ChoicePort
        {
            public uint id;
            public Port port;
            public TextField choiceText;
        }
        public List<ChoicePort> choicePorts { get; protected set; }

        public List<uint> conditionIds { get; protected set; }

        protected VisualElement variableContainer;
        public TextField speakerTextField { get; set; }
        public TextField dialogueTextField { get; set; }
        protected Port titleNextNodePort;

        protected uint choicePortId;
        protected uint conditionPortId;

        public delegate void NodeChangeEventHandler();
        public event NodeChangeEventHandler OnNodeChange;
        protected bool changed;
        private bool isCollapsed;


        public DialogueGraphNode()
        {
            title = "Basic Dialogue";
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueNodeStyle"));
            AddToClassList("dialogueGraphNode");    // USS style

            // create a container element to hold the variables; this allows for 
            // specific styles to be applied only to this area of the node.
            variableContainer = new VisualElement();
            variableContainer.name = "variable-container";
            mainContainer.Insert(1, variableContainer);
            titleContainer.RegisterCallback<MouseDownEvent>(x =>
            {
                IsCollapsed = !isCollapsed;
            });


            // field for speaker
            var speakerFieldLabel = new Label("Speaker Name");
            speakerFieldLabel.name = "dialogue-speaker-field-label";
            variableContainer.Add(speakerFieldLabel);
            speakerTextField = new TextField
            {
                name = "dialogue-speaker-field",
                multiline = false
            };
            variableContainer.Add(speakerTextField);

            // field for dialogue window text
            var dialogueFieldLabel = new Label("Dialogue Text");
            dialogueFieldLabel.name = "dialogue-text-field-label";
            variableContainer.Add(dialogueFieldLabel);
            dialogueTextField = new TextField
            {
                name = "dialogue-text-field",
                multiline = true
            };
            variableContainer.Add(dialogueTextField);

            // add ports to title container for dialogue node progression
            var prevDialogueNodePort = AddPort("", typeof(DialogueGraphNode), titleContainer, true, Port.Capacity.Multi, "prev-dialogue-node-input", 0);
            prevDialogueNodePort.AddToClassList("dialogueProgressPort");
            prevDialogueNodePort.tooltip = "Connect previous dialogue node here";

            titleNextNodePort = AddPort("", typeof(DialogueGraphNode), titleContainer, false, Port.Capacity.Multi, "next-dialogue-node-output");
            titleNextNodePort.AddToClassList("dialogueProgressPort");
            titleNextNodePort.tooltip = "Connect to next dialogue node (if not using dialogue choices)";

            // add button to input container to allow the addition of ports
            var addInputPortsButton = new Button(OnAddInputPortButtonClick);
            addInputPortsButton.text = "Add Condition";
            inputContainer.Add(addInputPortsButton);

            choicePortId = conditionPortId = 0;
            choicePorts = new List<ChoicePort>();
            conditionIds = new List<uint>();

            // add button to output container for dialogue choices to be added
            var addOutputPortsButton = new Button(OnAddOutputPortButtonClick);
            addOutputPortsButton.text = "Add Choice";
            outputContainer.Add(addOutputPortsButton);
        }

        protected void OnAddInputPortButtonClick()
        {
            AddConditionPort(conditionPortId);
            conditionPortId++;
            OnNodeChange?.Invoke();
        }

        protected void AddConditionPort(uint id)
        {
            // 1. create panel to parent the port and delete button; add to input container
            var conditionPortPanel = new VisualElement()
            {
                name = "dialogue-condition-panel"
            };
            inputContainer.Add(conditionPortPanel);

            // 2. add input port
            Port port = AddPort(
                "Boolean",
                typeof(bool),
                conditionPortPanel,
                true,
                Port.Capacity.Single,
                "dialogue-condition-input-port-" + id.ToString()
                );
            port.tooltip = "Connect a boolean value";
            port.AddToClassList("dialogueConditionInputPort");

            // 3. add delete button
            var deleteButton = new Button(() =>
            {
                port.DisconnectAll();
                conditionIds.Remove(id);
                inputContainer.Remove(conditionPortPanel);
                OnNodeChange?.Invoke();
            });
            deleteButton.name = "dialogue-condition-delete-button";
            deleteButton.text = "X";
            deleteButton.tooltip = "Delete";
            conditionPortPanel.Add(deleteButton);

            conditionIds.Add(id);
        }

        protected void OnAddOutputPortButtonClick()
        {
            AddChoicePort(choicePortId);
            choicePortId++;
            titleNextNodePort.SetEnabled(false);
            OnNodeChange?.Invoke();

            // if only one choice is present, add another one
            if (choicePorts.Count < 2)
            {
                OnAddOutputPortButtonClick();
            }
        }

        protected void AddChoicePort(uint id)
        {
            AddChoicePort(id, "<type choice text here>");
        }

        protected void AddChoicePort(uint id, string text)
        {
            // 1. create panel to parent the port and delete button; add to output container
            var choicePortPanel = new VisualElement()
            {
                name = "dialogue-choice-panel"
            };
            outputContainer.Add(choicePortPanel);

            ChoicePort choicePort = new ChoicePort();

            // 2. add delete button
            var deleteButton = new Button(() =>
            {
                choicePort.port.DisconnectAll();
                choicePorts.Remove(choicePort);
                titleNextNodePort.SetEnabled(choicePorts.Count == 0);
                outputContainer.Remove(choicePortPanel);
                OnNodeChange?.Invoke();
            });
            deleteButton.name = "dialogue-choice-delete-button";
            deleteButton.text = "X";
            deleteButton.tooltip = "Delete";
            choicePortPanel.Add(deleteButton);

            // 3. add text field for choice text
            var choiceTextField = new TextField()
            {
                multiline = true,
                value = text,
                name = "dialogue-choice-text-field-" + id.ToString()
            };
            choiceTextField.AddToClassList("dialogueChoiceTextField");
            choiceTextField.RegisterValueChangedCallback((evt) =>
            {
                OnNodeChange?.Invoke();
            });
            choicePort.choiceText = choiceTextField;
            choicePortPanel.Add(choiceTextField);

            // 4. add output port
            Port port = AddPort(
                "",
                typeof(DialogueGraphNode),
                choicePortPanel,
                false,
                Port.Capacity.Multi,
                "dialogue-choice-output-port-" + id.ToString()
                );
            port.tooltip = "Connect to a Dialogue Node";
            port.AddToClassList("dialogueChoiceOutputPort");
            choicePort.port = port;


            // 5. add the new ChoicePort struct to the list
            choicePort.id = id;
            choicePorts.Add(choicePort);
        }

        public void InitializeFromData(DialogueNodeData data)
        {
            // transfer basic dialogue data over to new node
            speakerTextField.value = data._speakerName;
            dialogueTextField.value = data._dialogueText;

            // add condition ports
            for (int i = 0; i < data._conditionPortIds.Count; i++)
            {
                AddConditionPort(data._conditionPortIds[i]);
                if (conditionPortId <= data._conditionPortIds[i])
                {
                    conditionPortId = data._conditionPortIds[i] + 1;
                }
            }

            // add choice ports
            for (int i = 0; i < data._choicePorts.Count; i++)
            {
                AddChoicePort(data._choicePorts[i]._portId, data._choicePorts[i]._choiceText);
                if (choicePortId <= data._choicePorts[i]._portId)
                {
                    choicePortId = data._choicePorts[i]._portId + 1;
                }
            }

            // transfer standard GraphNode data, add to graph
            NodeGuid = data._nodeGuid;
            SetPosition(new Rect(data._nodePosition, new Vector2(1, 1)));
        }

        public bool IsCollapsed
        {
            get
            {
                return isCollapsed;
            }
            set
            {
                variableContainer.SetEnabled(!value);
                isCollapsed = value;
            }
        }
    }

    public class AdvDialogueNode : DialogueGraphNode
    {
        public Vector3Field cameraPosField { get; set; }
        public Vector3Field cameraRotField { get; set; }
        public FloatField lerpTimeField { get; set; }

        public AdvDialogueNode()
        {
            title = "Adv. Dialogue";
            AddToClassList("advDialogueNode");

            // field for camera position
            var cameraPosLabel = new Label("Camera World Pos");
            variableContainer.Add(cameraPosLabel);
            cameraPosField = new Vector3Field();
            variableContainer.Add(cameraPosField);

            // field for camera rotation
            var cameraRotLabel = new Label("Camera World Rot (Angle)");
            variableContainer.Add(cameraRotLabel);
            cameraRotField = new Vector3Field();
            variableContainer.Add(cameraRotField);

            // field for lerp time
            lerpTimeField = new FloatField("Lerp Time (in seconds)");
            lerpTimeField.tooltip = "Determines how long the camera should take to move to the new position and rotation.";
            variableContainer.Add(lerpTimeField);
        }

        public void InitializeFromData(AdvDialogueNodeData data)
        {
            // transfer basic dialogue data over to new node
            speakerTextField.value = data._speakerName;
            dialogueTextField.value = data._dialogueText;

            // transfer advanced dialogue data over to new node
            cameraPosField.value = data._cameraPos;
            cameraRotField.value = data._cameraRot;
            lerpTimeField.value = data._lerpTime;

            // add condition ports
            for (int i = 0; i < data._conditionPortIds.Count; i++)
            {
                AddConditionPort(data._conditionPortIds[i]);
                if (conditionPortId <= data._conditionPortIds[i])
                {
                    conditionPortId = data._conditionPortIds[i] + 1;
                }
            }

            // add choice ports
            for (int i = 0; i < data._choicePorts.Count; i++)
            {
                AddChoicePort(data._choicePorts[i]._portId, data._choicePorts[i]._choiceText);
                if (choicePortId <= data._choicePorts[i]._portId)
                {
                    choicePortId = data._choicePorts[i]._portId + 1;
                }
            }

            // transfer standard GraphNode data, add to graph
            NodeGuid = data._nodeGuid;
            SetPosition(new Rect(data._nodePosition, new Vector2(1, 1)));
        }
    }

    public class CinematicDialogueNode : DialogueGraphNode
    {
        public ObjectField timelineField { get; set; }

        public CinematicDialogueNode()
        {
            title = "Cinematic";
            AddToClassList("cinematicDialogueNode");

            // we don't need the speaker and dialogue text fields; remove them and their labels
            variableContainer.Clear();

            // field for timeline asset to play animated scene
            var timelineFieldLabel = new Label("Animated Timeline Asset");
            variableContainer.Add(timelineFieldLabel);

            timelineField = new ObjectField()
            {
                objectType = typeof(TimelineAsset),
                allowSceneObjects = false
            };
            variableContainer.Add(timelineField);

            outputContainer.visible = false;
        }

        public void InitializeFromData(CinematicDialogueNodeData data)
        {
            // transfer basic dialogue data over to new node
            speakerTextField.value = data._speakerName;
            dialogueTextField.value = data._dialogueText;

            // transfer cinematic dialogue data over to new node
            timelineField.value = data._timelineAsset;

            // add condition ports
            for (int i = 0; i < data._conditionPortIds.Count; i++)
            {
                AddConditionPort(data._conditionPortIds[i]);
                if (conditionPortId <= data._conditionPortIds[i])
                {
                    conditionPortId = data._conditionPortIds[i] + 1;
                }
            }

            // add choice ports
            for (int i = 0; i < data._choicePorts.Count; i++)
            {
                AddChoicePort(data._choicePorts[i]._portId, data._choicePorts[i]._choiceText);
                if (choicePortId <= data._choicePorts[i]._portId)
                {
                    choicePortId = data._choicePorts[i]._portId + 1;
                }
            }

            // transfer standard GraphNode data, add to graph
            NodeGuid = data._nodeGuid;
            SetPosition(new Rect(data._nodePosition, new Vector2(1, 1)));
        }
    }
}
