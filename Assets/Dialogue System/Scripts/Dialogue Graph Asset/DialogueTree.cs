using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomSystem.DialogueSystem
{
    public class DialogueTree
    {
        public Dictionary<uint, DialogueNode> nodes { get; protected set; }
        public DialogueNode currentNode;

        private uint startNodeId;
        private List<uint> startNodeChildren;


        public DialogueTree(DialogueGraphAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("Dialogue asset sent to tree constructor was null!");
            }

            startNodeId = 0;
            startNodeChildren = new List<uint>();

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

            //  Before iterating through all edges, dissolve the redirector nodes first
            IEnumerable<NodeLinkData> linkData = asset.GetNodeLinksWithoutRedirects();

            //  2. iterate through list of links/edges and connect the nodes
            foreach (NodeLinkData edge in linkData)
            {
                bool inputIsDialogueNode, outputIsDialogueNode;
                inputIsDialogueNode = nodes.ContainsKey(edge._inputNodeGuid);
                outputIsDialogueNode = nodes.ContainsKey(edge._outputNodeGuid);

                //  if both GUIDs belong to dialogue nodes, then connect the nodes together in the dialogue tree
                if (outputIsDialogueNode && inputIsDialogueNode)
                {
                    DialogueNode outputNode = nodes[edge._outputNodeGuid];
                    DialogueNode inputNode = nodes[edge._inputNodeGuid];

                    // if this a connection from a choice, connect to the choice obj instead
                    if (edge._outputElementName.Contains("dialogue-choice-output-port"))
                    {
                        uint choiceId;
                        if (uint.TryParse(edge._outputElementName.Split('-')[4], out choiceId))
                        {
                            // this "connects" the input node to the dialogue choice
                            outputNode.choices[choiceId].childNodes.Add(inputNode);
                        }
                        else
                        {
                            Debug.LogError("Choice id parse failed in tree construction. Port element name: " + edge._outputElementName);
                        }
                    }
                    else
                    {
                        // "connects" the next node (input node) to the current node (output node)
                        outputNode.childNodes.Add(inputNode);
                    }
                }
                else if (inputIsDialogueNode)
                {
                    // find the starting dialogue node
                    if (edge._outputNodeGuid == startNodeId)
                    {
                        startNodeChildren.Add(edge._inputNodeGuid);
                        //startNode = nodes[edge._inputNodeGuid];
                    }
                    else
                    {
                        // if edge inputs to a dialogue node, but doesn't come from the start node,
                        // then it HAS to be from a node that outputs a boolean
                        NodeCondition condition = BuildNodeCondition(asset, edge, false);

                        if (condition != null)
                        {
                            nodes[edge._inputNodeGuid].conditions.Add(condition);
                        }
                    }
                }
            }
        }

        private NodeCondition BuildNodeCondition(in DialogueGraphAsset asset, in NodeLinkData tracedEdge, bool isOutputInversed)
        {
            NodeCondition condition = null;

            switch (tracedEdge._outputElementName)
            {
                // check for Compare nodes
                case "bool-compare-output":
                    {
                        BooleanComparisonNodeData data = asset.GetNodeDataByGuid(tracedEdge._outputNodeGuid) as BooleanComparisonNodeData;

                        switch (data._nodeType)
                        {
                            case "IntComparisonNode":
                                {
                                    condition = BuildComparisonCondition<int>(asset, data);
                                    break;
                                }
                            case "FloatComparisonNode":
                                {
                                    condition = BuildComparisonCondition<float>(asset, data);
                                    break;
                                }
                        }

                        break;
                    }

                // check for Logic gate nodes 
                case "logic-output":
                    {
                        BooleanLogicNodeData data = asset.GetNodeDataByGuid(tracedEdge._outputNodeGuid) as BooleanLogicNodeData;
                        condition = BuildLogicCondition(asset, data);

                        break;
                    }

                // check for Bool Getter nodes    
                case "accessor-bool-output":
                    {
                        AccessorNodeData nodeData = asset.GetNodeDataByGuid(tracedEdge._outputNodeGuid) as AccessorNodeData;

                        condition = new AccessedBoolCondition(nodeData._scriptableObj, nodeData._chosenPropertyString);

                        break;
                    }

                // check for NOT nodes    
                case "logic-not-output":
                    {
                        IEnumerable<NodeLinkData> linkData = asset.GetNodeLinksWithoutRedirects();

                        foreach (NodeLinkData edge in linkData)
                        {
                            if (edge._inputNodeGuid == tracedEdge._outputNodeGuid)
                            {
                                return BuildNodeCondition(asset, edge, !isOutputInversed);
                            }
                        }

                        break;
                    }
            }

            if (condition != null)
            {
                condition.IsOutputInversed = isOutputInversed;
            }

            return condition;
        }

        private ComparisonCondition<T> BuildComparisonCondition<T>(in DialogueGraphAsset asset, in BooleanComparisonNodeData nodeData) where T : IComparable<T>
        {
            ScriptableObject leftObj = null;
            ScriptableObject rightObj = null;
            string leftPropertyName = "";
            string rightPropertyName = "";
            T leftOperand = default(T);
            T rightOperand = default(T);

            IEnumerable<NodeLinkData> linkData = asset.GetNodeLinksWithoutRedirects();
            bool leftSearchFinished, rightSearchFinished;
            leftSearchFinished = rightSearchFinished = false;

            // iterate through the edges and 
            // find edges where the comparison node is the input (receiving) node
            foreach (NodeLinkData edge in linkData)
            {
                if (edge._inputNodeGuid == nodeData._nodeGuid)
                {
                    // determine which side of the comparison we're looking at here
                    bool isLeftOperand = edge._inputElementName == "bool-compare-input-1";

                    // determine if output (sending) node is either raw value node
                    // or if it is a getter node with an obj ref
                    NodeData otherNode = asset.GetNodeDataByGuid(edge._outputNodeGuid);

                    switch (otherNode._nodeType)
                    {
                        case "IntValueNode":
                            {
                                IntValNodeData castData = otherNode as IntValNodeData;

                                if (isLeftOperand)
                                {
                                    leftOperand = (T)(object)castData._intVal;
                                    leftSearchFinished = true;
                                }
                                else
                                {
                                    rightOperand = (T)(object)castData._intVal;
                                    rightSearchFinished = true;
                                }

                                break;
                            }
                        case "FloatValueNode":
                            {
                                FloatValNodeData castData = otherNode as FloatValNodeData;

                                if (isLeftOperand)
                                {
                                    leftOperand = (T)(object)castData._floatVal;
                                    leftSearchFinished = true;
                                }
                                else
                                {
                                    rightOperand = (T)(object)castData._floatVal;
                                    rightSearchFinished = true;
                                }

                                break;
                            }
                        case "AccessorNode":
                            {
                                AccessorNodeData castData = otherNode as AccessorNodeData;

                                if (isLeftOperand)
                                {
                                    leftObj = castData._scriptableObj;
                                    leftPropertyName = castData._chosenPropertyString;
                                    leftSearchFinished = true;
                                }
                                else
                                {
                                    rightObj = castData._scriptableObj;
                                    rightPropertyName = castData._chosenPropertyString;
                                    rightSearchFinished = true;
                                }

                                break;
                            }
                    }

                    // if both operands have been found, break out of the loop through edges
                    if (leftSearchFinished && rightSearchFinished)
                    {
                        break;
                    }
                }
            }

            // create and return a comparison condition object
            bool leftObjRefFound = leftObj != null;
            bool rightObjRefFound = rightObj != null;

            if (leftObjRefFound || rightObjRefFound)
            {
                if (leftObjRefFound && rightObjRefFound)
                {
                    return new ComparisonCondition<T>(
                        (ComparisonCondition<T>.CompareOperator)nodeData._comparisonEnumVal,
                        leftObj, leftPropertyName,
                        rightObj, rightPropertyName);
                }
                else if (leftObjRefFound)
                {
                    return new ComparisonCondition<T>(
                        (ComparisonCondition<T>.CompareOperator)nodeData._comparisonEnumVal,
                        leftObj, leftPropertyName,
                        rightOperand);
                }
                else
                {
                    return new ComparisonCondition<T>(
                        (ComparisonCondition<T>.CompareOperator)nodeData._comparisonEnumVal,
                        leftOperand,
                        rightObj, rightPropertyName);
                }
            }
            else
            {
                return new ComparisonCondition<T>(
                        (ComparisonCondition<T>.CompareOperator)nodeData._comparisonEnumVal,
                        leftOperand,
                        rightOperand);
            }
        }

        private LogicCondition BuildLogicCondition(in DialogueGraphAsset asset, in BooleanLogicNodeData nodeData)
        {
            // create condition obj using data's enum value
            LogicCondition condition = new LogicCondition((LogicCondition.LogicOperator)nodeData._logicEnumVal);

            // get all of the edges, without redirects
            IEnumerable<NodeLinkData> linkData = asset.GetNodeLinksWithoutRedirects();

            // find input node condition objects
            foreach (NodeLinkData edge in linkData)
            {
                // if edge is connected to the logic node
                if (edge._inputNodeGuid == nodeData._nodeGuid)
                {
                    // first two port IDs (0 & 1) are always there, but additional ports
                    // could have been added, and their values are stored in the nodeData's list
                    for (int i = 0; i < 2 + nodeData._additionalInputPortIds.Count; i++)
                    {
                        uint id = (i < 2) ? (uint)i : nodeData._additionalInputPortIds[i - 2];
                        string portElementId = "bool-input-port-" + id.ToString();

                        if (edge._inputElementName == portElementId)
                        {
                            NodeCondition input = BuildNodeCondition(asset, edge, false);

                            if (input != null)
                            {
                                condition.AddInput(input);
                            }

                            break;
                        }
                    }
                }
            }

            return condition;
        }

        public DialogueNode StartNode
        {
            get
            {
                for (int i = 0; i < startNodeChildren.Count; i++)
                {
                    if (nodes[startNodeChildren[i]].IsAvailableForUse)
                    {
                        return nodes[startNodeChildren[i]];
                    }
                }

                return null;
            }
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

            public DialogueNode FirstAvailableChildNode
            {
                get
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].IsAvailableForUse)
                        {
                            return childNodes[i];
                        }
                    }

                    return null;
                }
            }
        }

        public List<DialogueNode> childNodes;
        public List<NodeCondition> conditions;
        public Dictionary<uint, DialogueChoice> choices;

        public string speakerName { get; protected set; }
        public string dialogueText { get; protected set; }
        public uint guid { get; protected set; }

        public DialogueNode()
        {
            conditions = new List<NodeCondition>();
            childNodes = new List<DialogueNode>();
            choices = new Dictionary<uint, DialogueChoice>();
        }

        public DialogueNode(DialogueNodeData data)
        {
            conditions = new List<NodeCondition>();
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

        /// <summary>
        /// Whether or not this dialogue node can be used in the current player interaction. 
        /// </summary>
        public bool IsAvailableForUse
        {
            get
            {
                bool isAvailable = true;

                foreach (NodeCondition condition in conditions)
                {
                    if (!condition.Evaluate())
                    {
                        isAvailable = false;
                        break;
                    }
                }

                return isAvailable;
            }
        }

        public DialogueNode FirstAvailableChildNode
        {
            get
            {
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (childNodes[i].IsAvailableForUse)
                    {
                        return childNodes[i];
                    }
                }

                return null;
            }
        }

        public bool HasAvailableChoices
        {
            get
            {
                foreach (KeyValuePair<uint, DialogueNode.DialogueChoice> choiceEntry in choices)
                {
                    if (choiceEntry.Value.FirstAvailableChildNode != null)
                    {
                        return true;
                    }
                }

                return false;
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