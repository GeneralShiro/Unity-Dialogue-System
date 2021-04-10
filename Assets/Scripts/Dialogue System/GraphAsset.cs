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

            return ret;
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

        public AccessorNodeData() { }
    }
}
