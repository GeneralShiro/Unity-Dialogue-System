using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomSystem.DialogueSystem
{
    public class DialogueTree
    {
        public List<DialogueNode> nodes { get; protected set; }

        public DialogueTree()
        {
            nodes = new List<DialogueNode>();
        }

        public DialogueTree(DialogueGraphAsset asset)
        {
            nodes = new List<DialogueNode>();

            
        }
    }

    public class DialogueNode
    {
        public List<DialogueNode> childNodes;

        public string speakerName { get; protected set; }
        public string dialogueText { get; protected set; }

        public DialogueNode()
        {
            childNodes = new List<DialogueNode>();
        }

        public DialogueNode(DialogueNodeData data)
        {
            childNodes = new List<DialogueNode>();

            speakerName = data._speakerName;
            dialogueText = data._dialogueText;
        }
    }

    public class AdvDialogueNode : DialogueNode
    {
        public Vector3 cameraWorldPos { get; protected set; }
        public Vector3 cameraWorldRot { get; protected set; }

        public AdvDialogueNode() { }

        public AdvDialogueNode(AdvDialogueNodeData data)
        {
            cameraWorldPos = data._cameraPos;
            cameraWorldRot = data._cameraRot;
        }
    }

    public class CinematicDialogueNode : DialogueNode
    {
        public TimelineAsset timelineAsset { get; protected set; }

        public CinematicDialogueNode() { }

        public CinematicDialogueNode(CinematicDialogueNodeData data)
        {
            timelineAsset = data._timelineAsset;
        }
    }
}