using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


namespace CustomGraphEditors.DialogueSystem
{
	public class DialogueGraphNode : GraphNode
	{
		protected VisualElement variableContainer;

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
			var speakerTextField = new TextField { multiline = false };
			variableContainer.Add(speakerTextField);

			// field for dialogue window text
			var dialogueFieldLabel = new Label("Dialogue Text");
			variableContainer.Add(dialogueFieldLabel);
			var dialogueTextField = new TextField { name = "dialogue-text-field", multiline = true };
			variableContainer.Add(dialogueTextField);

			// add ports
			AddInputPort("Prev Node", typeof(DialogueGraphNode), Port.Capacity.Multi);
			AddOutputPort("Next Node", typeof(DialogueGraphNode), Port.Capacity.Multi);
		}
	}

	public class AdvDialogueNode : DialogueGraphNode
	{
		public AdvDialogueNode()
		{
			title = "Advanced Dialogue Node";
			AddToClassList("advDialogueNode");

			// field for camera position
			var cameraPosLabel = new Label("Camera World Pos");
			variableContainer.Add(cameraPosLabel);
			var cameraPosField = new Vector3Field();
			variableContainer.Add(cameraPosField);

			// field for camera rotation
			var cameraRotLabel = new Label("Camera World Rot (Angle)");
			variableContainer.Add(cameraRotLabel);
			var cameraRotField = new Vector3Field();
			variableContainer.Add(cameraRotField);
		}
	}

	public class CinematicDialogueNode : DialogueGraphNode
	{
		public CinematicDialogueNode()
		{
			title = "Cinematic Dialogue Node";
			AddToClassList("cinematicDialogueNode");

			// field for timeline asset to play animated scene
			var timelineFieldLabel = new Label("Animated Timeline Asset");
			variableContainer.Add(timelineFieldLabel);

			var timelineField = new ObjectField()
			{
				objectType = typeof(TimelineAsset),
				allowSceneObjects = false
			};
			variableContainer.Add(timelineField);
		}
	}
}
