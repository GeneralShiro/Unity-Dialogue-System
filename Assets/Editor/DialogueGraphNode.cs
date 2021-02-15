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

            var nextDialogueNodePort = AddPort("", typeof(DialogueGraphNode), titleContainer, false, Port.Capacity.Multi, "next-dialogue-node-input");
            nextDialogueNodePort.AddToClassList("dialogueProgressPort");
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
