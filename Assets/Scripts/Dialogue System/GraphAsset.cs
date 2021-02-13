using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CustomGraphEditors
{
    public abstract class GraphAsset : ScriptableObject
    {
        public List<NodeLinkData> nodeLinkData;
        //public List<NodeData> nodeData;
        public List<BooleanNodeData> booleanNodeData;

        public virtual uint GetNewGUID()
        {
            uint ret = 0;

            for (int i = 0; i < booleanNodeData.Count; i++)
            {
                if (ret <= booleanNodeData[i]._nodeGuid)
                {
                    ret = booleanNodeData[i]._nodeGuid + 1;
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
    public class BooleanNodeData : NodeData
    {
        public int _booleanOpEnumVal;
    }
}
