using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


namespace CustomEditors.DialogueSystem
{
    public class DialogueGraphNode : GraphNode
    {
        protected VisualElement variableContainer;
        public TextField speakerTextField { get; set; }
        public TextField dialogueTextField { get; set; }
        protected Port titleNextNodePort;

        protected uint numOfChoices;

        public DialogueGraphNode()
        {
            title = "Basic Dialogue Node";
            AddToClassList("dialogueGraphNode");    // USS style

            // create a container element to hold the variables; this allows for 
            // specific styles to be applied only to this area of the node.
            variableContainer = new VisualElement();
            variableContainer.name = "variable-container";
            mainContainer.Insert(1, variableContainer);

            // field for speaker
            var speakerFieldLabel = new Label("Speaker Name");
            variableContainer.Add(speakerFieldLabel);
            speakerTextField = new TextField { multiline = false };
            variableContainer.Add(speakerTextField);

            // field for dialogue window text
            var dialogueFieldLabel = new Label("Dialogue Text");
            variableContainer.Add(dialogueFieldLabel);
            dialogueTextField = new TextField { name = "dialogue-text-field", multiline = true };
            variableContainer.Add(dialogueTextField);

            // get rid of the collapse button
            titleButtonContainer.RemoveFromHierarchy();

            // add ports to title container for dialogue node progression
            var prevDialogueNodePort = AddPort("", typeof(DialogueGraphNode), titleContainer, true, Port.Capacity.Multi, "prev-dialogue-node-input", 0);
            prevDialogueNodePort.AddToClassList("dialogueProgressPort");

            titleNextNodePort = AddPort("", typeof(DialogueGraphNode), titleContainer, false, Port.Capacity.Multi, "next-dialogue-node-input");
            titleNextNodePort.AddToClassList("dialogueProgressPort");

            // add button to input container to allow the addition of ports
            var addInputPortsButton = new Button(OnAddInputPortButtonClick);
            addInputPortsButton.text = "Add Condition";
            inputContainer.Add(addInputPortsButton);

            numOfChoices = 0;

            // add button to output container for dialogue choices to be added
            var addOutputPortsButton = new Button(OnAddOutputPortButtonClick);
            addOutputPortsButton.text = "Add Choice";
            outputContainer.Add(addOutputPortsButton);
        }

        protected void OnAddInputPortButtonClick()
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
                "dialogue-condition-input-port"
                );
            port.tooltip = "Connect a boolean value";

            // 3. add delete button
            var deleteButton = new Button(() =>
            {
                inputContainer.Remove(conditionPortPanel);
            });
            deleteButton.name = "dialogue-condition-delete-button";
            deleteButton.text = "X";
            conditionPortPanel.Add(deleteButton);
        }

        protected void OnAddOutputPortButtonClick()
        {
            // TODO: add two choice panels if none are present, otherwise only add one. 
            // Also show warning if title container output port is being used; if choice outputs are present, the title bar output port won't be used.

            // 1. create panel to parent the port and delete button; add to output container
            var choicePortPanel = new VisualElement()
            {
                name = "dialogue-choice-panel"
            };
            outputContainer.Add(choicePortPanel);

            // 2. add delete button
            var deleteButton = new Button(() =>
            {
                outputContainer.Remove(choicePortPanel);
                numOfChoices--;
                titleNextNodePort.SetEnabled(numOfChoices == 0);
            });
            deleteButton.name = "dialogue-choice-delete-button";
            deleteButton.text = "X";
            choicePortPanel.Add(deleteButton);

            // 3. add text field for choice text
            var choiceTextField = new TextField()
            {
                multiline = true,
                value = "<type choice text here>",
                name = "dialogue-choice-text-field"
            };
            choicePortPanel.Add(choiceTextField);

            // 4. add output port
            Port port = AddPort(
                "",
                typeof(DialogueGraphNode),
                choicePortPanel,
                false,
                Port.Capacity.Multi,
                "dialogue-choice-output-port"
                );
            port.tooltip = "Connect to a Dialogue Node";

            numOfChoices++;
            titleNextNodePort.SetEnabled(false);

            // if only one choice is present, add another one
            if (numOfChoices < 2)
            {
                OnAddOutputPortButtonClick();
            }
        }
    }

    public class AdvDialogueNode : DialogueGraphNode
    {
        public Vector3Field cameraPosField { get; set; }
        public Vector3Field cameraRotField { get; set; }

        public AdvDialogueNode()
        {
            title = "Advanced Dialogue Node";
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
        }
    }

    public class CinematicDialogueNode : DialogueGraphNode
    {
        public ObjectField timelineField { get; set; }

        public CinematicDialogueNode()
        {
            title = "Cinematic Dialogue Node";
            AddToClassList("cinematicDialogueNode");

            // field for timeline asset to play animated scene
            var timelineFieldLabel = new Label("Animated Timeline Asset");
            variableContainer.Add(timelineFieldLabel);

            timelineField = new ObjectField()
            {
                objectType = typeof(TimelineAsset),
                allowSceneObjects = false
            };
            variableContainer.Add(timelineField);
        }
    }
}
