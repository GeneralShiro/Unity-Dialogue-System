using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomSystem.DialogueSystem
{
    public class DialogueTree
    {
        public Dictionary<uint, DialogueNode> nodes { get; protected set; }
        public DialogueNode startNode { get; protected set; }
        public DialogueNode currentNode;

        public DialogueTree()
        {
            nodes = new Dictionary<uint, DialogueNode>();
        }

        public DialogueTree(DialogueGraphAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("Dialogue asset sent to tree constructor was null!");
            }

            uint startNodeId = 0;

            //  1. get all dialogue node data first; dump into dictionary of constructed dialogue node objects
            nodes = new Dictionary<uint, DialogueNode>();

            //      1a. create the simple dialogue nodes
            foreach (DialogueNodeData data in asset.dialogueNodeData)
            {
                nodes.Add(data._nodeGuid, new DialogueNode(data));
            }

            //      1b. create the advanced dialogue nodes
            foreach (AdvDialogueNodeData data in asset.advDialogueNodeData)
            {
                nodes.Add(data._nodeGuid, new AdvDialogueNode(data));
            }

            //      1c. create the cinematic dialogue nodes
            foreach (CinematicDialogueNodeData data in asset.cinematicDialogueNodeData)
            {
                nodes.Add(data._nodeGuid, new CinematicDialogueNode(data));
            }

            //      1d. find the start node
            foreach (NodeData data in asset.graphNodeData)
            {
                if (data._nodeType == "StartNode")
                {
                    startNodeId = data._nodeGuid;
                }
            }

            //  2. iterate through list of links/edges and connect the nodes
            foreach (NodeLinkData data in asset.nodeLinkData)
            {
                // check if both GUIDs belong to dialogue nodes first
                if (nodes.ContainsKey(data._outputNodeGuid) && nodes.ContainsKey(data._inputNodeGuid))
                {
                    DialogueNode outputNode = nodes[data._outputNodeGuid];
                    DialogueNode inputNode = nodes[data._inputNodeGuid];

                    // if this a connection from a choice, connect to the choice obj instead
                    if (data._outputElementName.Contains("dialogue-choice-output-port"))
                    {
                        uint choiceId;
                        if (uint.TryParse(data._outputElementName.Split('-')[4], out choiceId))
                        {
                            // this "connects" the input node to the dialogue choice
                            outputNode.choices[choiceId].childNodes.Add(inputNode);
                        }
                        else
                        {
                            Debug.LogError("Choice id parse failed in tree construction. Port element name: " + data._outputElementName);
                        }
                    }
                    else
                    {
                        // "connects" the next node (input node) to the current node (output node)
                        outputNode.childNodes.Add(inputNode);
                    }
                }
                else    // handle edges involving non-dialogue nodes
                {
                    // find the starting dialogue node
                    if (data._outputNodeGuid == startNodeId && nodes.ContainsKey(data._inputNodeGuid))
                    {
                        startNode = nodes[data._inputNodeGuid];
                    }
                }
            }

            //  TODO: figure out how to handle the 'conditions'
        }
    }

    public class DialogueNode
    {
        public class DialogueChoice
        {
            public uint choiceId { get; protected set; }
            public string choiceText { get; protected set; }
            public List<DialogueNode> childNodes;

            public DialogueChoice() : this("") { }

            public DialogueChoice(string choiceText)
            {
                childNodes = new List<DialogueNode>();
                this.choiceText = choiceText;
            }

            public DialogueChoice(DialogueNodeData.ChoicePortData data)
            {
                childNodes = new List<DialogueNode>();
                choiceText = data._choiceText;
                choiceId = data._portId;
            }
        }

        public List<DialogueNode> childNodes;
        public Dictionary<uint, DialogueChoice> choices;
        public string speakerName { get; protected set; }
        public string dialogueText { get; protected set; }
        public uint guid { get; protected set; }

        public DialogueNode()
        {
            childNodes = new List<DialogueNode>();
            choices = new Dictionary<uint, DialogueChoice>();
        }

        public DialogueNode(DialogueNodeData data)
        {
            childNodes = new List<DialogueNode>();

            speakerName = data._speakerName;
            dialogueText = data._dialogueText;
            guid = data._nodeGuid;

            choices = new Dictionary<uint, DialogueChoice>();
            List<DialogueNodeData.ChoicePortData> choiceData = data._choicePorts;
            for (int i = 0; i < choiceData.Count; i++)
            {
                choices.Add(choiceData[i]._portId, new DialogueChoice(choiceData[i]));
            }
        }
    }

    public class AdvDialogueNode : DialogueNode
    {
        public Vector3 cameraWorldPos { get; protected set; }
        public Vector3 cameraWorldRot { get; protected set; }
        public float lerpTime { get; protected set; }

        public AdvDialogueNode() { }

        public AdvDialogueNode(AdvDialogueNodeData data)
        {
            childNodes = new List<DialogueNode>();

            speakerName = data._speakerName;
            dialogueText = data._dialogueText;
            guid = data._nodeGuid;

            cameraWorldPos = data._cameraPos;
            cameraWorldRot = data._cameraRot;
            lerpTime = data._lerpTime;

            choices = new Dictionary<uint, DialogueChoice>();
            List<DialogueNodeData.ChoicePortData> choiceData = data._choicePorts;
            for (int i = 0; i < choiceData.Count; i++)
            {
                choices.Add(choiceData[i]._portId, new DialogueChoice(choiceData[i]));
            }
        }
    }

    public class CinematicDialogueNode : DialogueNode
    {
        public TimelineAsset timelineAsset { get; protected set; }

        public CinematicDialogueNode() { }

        public CinematicDialogueNode(CinematicDialogueNodeData data)
        {
            childNodes = new List<DialogueNode>();

            speakerName = data._speakerName;
            dialogueText = data._dialogueText;
            guid = data._nodeGuid;

            timelineAsset = data._timelineAsset;

            choices = new Dictionary<uint, DialogueChoice>();
            List<DialogueNodeData.ChoicePortData> choiceData = data._choicePorts;
            for (int i = 0; i < choiceData.Count; i++)
            {
                choices.Add(choiceData[i]._portId, new DialogueChoice(choiceData[i]));
            }
        }
    }
}