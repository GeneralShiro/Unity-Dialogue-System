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

            return null;
        }

        public IEnumerable<NodeLinkData> GetNodeLinksWithoutRedirects()
        {
            Dictionary<int, int> redirectNodeLinks = new Dictionary<int, int>();

            // find the indices of every edge associated with an edge redirector
            for (int i = 0; i < edgeRedirectorData.Count; i++)
            {
                int indexInput, indexOutput;
                indexInput = indexOutput = (-i - 1);

                for (int j = 0; j < nodeLinkData.Count; j++)
                {
                    if (nodeLinkData[j]._outputNodeGuid == edgeRedirectorData[i]._nodeGuid)
                    {
                        indexOutput = j;

                        if (indexInput >= 0)
                        {
                            break;
                        }
                    }
                    else if (nodeLinkData[j]._inputNodeGuid == edgeRedirectorData[i]._nodeGuid)
                    {
                        indexInput = j;

                        if (indexOutput >= 0)
                        {
                            break;
                        }
                    }
                }

                // if at least one of the indices were found, add it to the dictionary; the negative values can be used to determine which edges to ignore
                if (indexInput >= 0 || indexOutput >= 0)
                {
                    redirectNodeLinks.Add(indexInput, indexOutput);
                }
            }

            // return the non-redirected edges via the IEnumerable
            for (int i = 0; i < nodeLinkData.Count; i++)
            {
                if (redirectNodeLinks.ContainsKey(i) || redirectNodeLinks.ContainsValue(i))
                {
                    // only handle these edges using the keys; this avoids returning the same edge twice (via the index stored in the 'value' of the pair)
                    if (redirectNodeLinks.ContainsKey(i))
                    {
                        // if the other index is valid, dissolve the two edges into one and return it
                        if (redirectNodeLinks[i] >= 0)
                        {
                            int otherLinkIndex = redirectNodeLinks[i];
                            NodeLinkData newLink = new NodeLinkData();

                            newLink._inputElementName = nodeLinkData[i]._inputElementName;
                            newLink._inputNodeGuid = nodeLinkData[i]._inputNodeGuid;
                            newLink._inputPortName = nodeLinkData[i]._inputPortName;

                            newLink._outputElementName = nodeLinkData[otherLinkIndex]._outputElementName;
                            newLink._outputNodeGuid = nodeLinkData[otherLinkIndex]._outputNodeGuid;
                            newLink._outputPortName = nodeLinkData[otherLinkIndex]._outputPortName;

                            yield return newLink;
                        }
                    }
                }
                else
                {
                    yield return nodeLinkData[i];
                }
            }
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
        public int _leftPortCapacityVal;
        public int _rightPortCapacityVal;
        public NodeLinkData _formerEdgeData;
    }
}
