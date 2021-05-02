using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CustomSystem
{
    public abstract class GraphAsset : ScriptableObject
    {
        public List<NodeLinkData> nodeLinkData;
        public List<NodeData> graphNodeData;

        public List<BooleanComparisonNodeData> booleanComparisonNodeData;
        public List<BooleanLogicNodeData> booleanLogicNodeData;

        public List<AccessorNodeData> accessorNodeData;
        public List<IntValNodeData> intValNodeData;
        public List<FloatValNodeData> floatValNodeData;

        public List<EdgeRedirectorData> edgeRedirectorData;


        public virtual uint GetNewGUID()
        {
            uint ret = 1;

            for (int i = 0; i < graphNodeData.Count; i++)
            {
                if (ret <= graphNodeData[i]._nodeGuid)
                {
                    ret = graphNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < booleanComparisonNodeData.Count; i++)
            {
                if (ret <= booleanComparisonNodeData[i]._nodeGuid)
                {
                    ret = booleanComparisonNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < booleanLogicNodeData.Count; i++)
            {
                if (ret <= booleanLogicNodeData[i]._nodeGuid)
                {
                    ret = booleanLogicNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < intValNodeData.Count; i++)
            {
                if (ret <= intValNodeData[i]._nodeGuid)
                {
                    ret = intValNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < floatValNodeData.Count; i++)
            {
                if (ret <= floatValNodeData[i]._nodeGuid)
                {
                    ret = floatValNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < accessorNodeData.Count; i++)
            {
                if (ret <= accessorNodeData[i]._nodeGuid)
                {
                    ret = accessorNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < edgeRedirectorData.Count; i++)
            {
                if (ret <= edgeRedirectorData[i]._nodeGuid)
                {
                    ret = edgeRedirectorData[i]._nodeGuid + 1;
                }
            }

            return ret;
        }

        public virtual NodeData GetNodeDataByGuid(uint guid)
        {
            for (int i = 0; i < graphNodeData.Count; i++)
            {
                if (guid == graphNodeData[i]._nodeGuid)
                {
                    return graphNodeData[i];
                }
            }

            for (int i = 0; i < booleanComparisonNodeData.Count; i++)
            {
                if (guid == booleanComparisonNodeData[i]._nodeGuid)
                {
                    return booleanComparisonNodeData[i];
                }
            }

            for (int i = 0; i < booleanLogicNodeData.Count; i++)
            {
                if (guid == booleanLogicNodeData[i]._nodeGuid)
                {
                    return booleanLogicNodeData[i];
                }
            }

            for (int i = 0; i < accessorNodeData.Count; i++)
            {
                if (guid == accessorNodeData[i]._nodeGuid)
                {
                    return accessorNodeData[i];
                }
            }

            for (int i = 0; i < intValNodeData.Count; i++)
            {
                if (guid == intValNodeData[i]._nodeGuid)
                {
                    return intValNodeData[i];
                }
            }

            for (int i = 0; i < floatValNodeData.Count; i++)
            {
                if (guid == floatValNodeData[i]._nodeGuid)
                {
                    return floatValNodeData[i];
                }
            }

            for (int i = 0; i < edgeRedirectorData.Count; i++)
            {
                if (guid == edgeRedirectorData[i]._nodeGuid)
                {
                    return edgeRedirectorData[i];
                }
            }

            return null;
        }

        public IEnumerable<NodeLinkData> GetNodeLinksWithoutRedirects()
        {
            // find the indices of every edge associated with an edge redirector
            for (int i = 0; i < edgeRedirectorData.Count; i++)
            {
                int leftEdgeIndex = -1;

                // find the left edge index first, since there's only one allowed
                for (int j = 0; j < nodeLinkData.Count; j++)
                {
                    NodeData leftNode = GetNodeDataByGuid(nodeLinkData[j]._outputNodeGuid);

                    if (leftNode != null)
                    {
                        // if right node is a redirect and left node is NOT a redirect, store the edge as the 'left edge'
                        if (nodeLinkData[j]._inputNodeGuid == edgeRedirectorData[i]._nodeGuid && leftNode._nodeType != "EdgeRedirector")
                        {
                            leftEdgeIndex = j;
                            break;
                        }
                    }
                }

                // return redirected edges, dissolved into one edge
                if (leftEdgeIndex >= 0)
                {
                    List<int> endEdgeIndices = FindEndsOfRedirectNode(edgeRedirectorData[i]._nodeGuid);

                    foreach (int rightEdgeIndex in endEdgeIndices)
                    {
                        NodeLinkData newLink = new NodeLinkData();

                        newLink._inputElementName = nodeLinkData[rightEdgeIndex]._inputElementName;
                        newLink._inputNodeGuid = nodeLinkData[rightEdgeIndex]._inputNodeGuid;
                        newLink._inputPortName = nodeLinkData[rightEdgeIndex]._inputPortName;

                        newLink._outputElementName = nodeLinkData[leftEdgeIndex]._outputElementName;
                        newLink._outputNodeGuid = nodeLinkData[leftEdgeIndex]._outputNodeGuid;
                        newLink._outputPortName = nodeLinkData[leftEdgeIndex]._outputPortName;

                        yield return newLink;
                    }
                }
            }

            // now return non-redirected edges
            for (int i = 0; i < nodeLinkData.Count; i++)
            {
                NodeData leftNode = GetNodeDataByGuid(nodeLinkData[i]._outputNodeGuid);
                NodeData rightNode = GetNodeDataByGuid(nodeLinkData[i]._inputNodeGuid);

                if (leftNode != null && rightNode != null)
                {
                    if (leftNode._nodeType != "EdgeRedirector" && rightNode._nodeType != "EdgeRedirector")
                    {
                        yield return nodeLinkData[i];
                    }
                }
            }
        }

        private List<int> FindEndsOfRedirectNode(uint redirectorID)
        {
            // prepare to collect multiple edge indices since redirector nodes can possibly connect to multiple nodes
            List<int> edgeIndices = new List<int>();

            for (int i = 0; i < nodeLinkData.Count; i++)
            {
                if (nodeLinkData[i]._outputNodeGuid == redirectorID)
                {
                    NodeData rightNode = GetNodeDataByGuid(nodeLinkData[i]._inputNodeGuid);

                    // if the right node is valid and the left node is the current redirector we're looking at...
                    if (rightNode != null)
                    {
                        // check for redirector on right side; call this func recursively
                        if (rightNode._nodeType == "EdgeRedirector")
                        {
                            List<int> foundEndEdges = FindEndsOfRedirectNode(rightNode._nodeGuid);

                            foreach (int edgeIndex in foundEndEdges)
                            {
                                edgeIndices.Add(edgeIndex);
                            }
                        }
                        // otherwise, non-redirector on right side 
                        else
                        {
                            edgeIndices.Add(i);
                        }
                    }
                }
            }

            return edgeIndices;
        }
    }

    [System.Serializable]
    public class NodeData
    {
        public uint _nodeGuid;
        public string _nodeType;
        public Vector2 _nodePosition;
    }

    [System.Serializable]
    public class NodeLinkData
    {
        public uint _inputNodeGuid;
        public uint _outputNodeGuid;
        public string _inputPortName;
        public string _inputElementName;
        public string _outputPortName;
        public string _outputElementName;

        public NodeLinkData() { }

        public NodeLinkData(NodeLinkData data)
        {
            _inputNodeGuid = data._inputNodeGuid;
            _outputNodeGuid = data._outputNodeGuid;
            _inputPortName = data._inputPortName;
            _inputElementName = data._inputElementName;
            _outputPortName = data._outputPortName;
            _outputElementName = data._outputElementName;
        }
    }

    [System.Serializable]
    public class BooleanComparisonNodeData : NodeData
    {
        public int _comparisonEnumVal;
    }

    [System.Serializable]
    public class BooleanLogicNodeData : NodeData
    {
        public int _logicEnumVal;
        public List<uint> _additionalInputPortIds;

        public BooleanLogicNodeData()
        {
            _additionalInputPortIds = new List<uint>();
        }
    }

    [System.Serializable]
    public class AccessorNodeData : NodeData
    {
        public ScriptableObject _scriptableObj;
        public string _chosenPropertyString;        // name of the property chosen from the scriptableobj
        public int _typeEnumVal;                    // correlates to 'UnityEditor.SerializedPropertyType' enum value
    }

    [System.Serializable]
    public class IntValNodeData : NodeData
    {
        public int _intVal;
    }

    [System.Serializable]
    public class FloatValNodeData : NodeData
    {
        public float _floatVal;
    }

    [System.Serializable]
    public class EdgeRedirectorData : NodeData
    {
        public System.Type _leftPortType;
        public System.Type _rightPortType;
        public int _rightPortCapacityVal;
    }
}
