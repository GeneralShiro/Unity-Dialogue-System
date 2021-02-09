using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CustomGraphEditors
{
    public abstract class GraphAsset : ScriptableObject
    {
        public List<NodeLinkData> nodeLinkData;
        public List<NodeData> nodeData;

        public uint GetNewGUID()
        {
            uint ret = 0;

            for (int i = 0; i < nodeData.Count; i++)
            {
                if (ret < nodeData[i].nodeGuid)
                {
                    ret = nodeData[i].nodeGuid;
                }
            }

            return ++ret;
        }
    }

    [System.Serializable]
    public class NodeLinkData
    {
        public uint inputNodeGuid;
        public uint outputNodeGuid;
        public string inputPortName;
        public string outputPortName;
    }

    [System.Serializable]
    public class NodeData
    {
        public uint nodeGuid;
        public string nodeType;
        public Vector2 nodePosition;
    }
}
