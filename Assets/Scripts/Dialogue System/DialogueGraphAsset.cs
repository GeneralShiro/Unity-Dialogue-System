using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomSystem.DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Custom Graph Asset/Dialogue Graph", order = 1)]
    public class DialogueGraphAsset : GraphAsset
    {
        public List<DialogueNodeData> dialogueNodeData;
        public List<AdvDialogueNodeData> advDialogueNodeData;
        public List<CinematicDialogueNodeData> cinematicDialogueNodeData;


        public DialogueGraphAsset()
        {
            graphNodeData = new List<NodeData>();
            nodeLinkData = new List<NodeLinkData>();

            edgeRedirectorData = new List<EdgeRedirectorData>();

            booleanLogicNodeData = new List<BooleanLogicNodeData>();
            booleanComparisonNodeData = new List<BooleanComparisonNodeData>();

            accessorNodeData = new List<AccessorNodeData>();
            
            dialogueNodeData = new List<DialogueNodeData>();
            advDialogueNodeData = new List<AdvDialogueNodeData>();
            cinematicDialogueNodeData = new List<CinematicDialogueNodeData>();
        }

        public override uint GetNewGUID()
        {
            uint ret = base.GetNewGUID();

            for (int i = 0; i < dialogueNodeData.Count; i++)
            {
                if (ret <= dialogueNodeData[i]._nodeGuid)
                {
                    ret = dialogueNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < advDialogueNodeData.Count; i++)
            {
                if (ret <= advDialogueNodeData[i]._nodeGuid)
                {
                    ret = advDialogueNodeData[i]._nodeGuid + 1;
                }
            }

            for (int i = 0; i < cinematicDialogueNodeData.Count; i++)
            {
                if (ret <= cinematicDialogueNodeData[i]._nodeGuid)
                {
                    ret = cinematicDialogueNodeData[i]._nodeGuid + 1;
                }
            }

            return ret;
        }
    }

    [System.Serializable]
    public class DialogueNodeData : NodeData
    {
        [System.Serializable]
        public struct ChoicePortData
        {
            public uint _portId;
            public string _choiceText;
        }
        public List<ChoicePortData> _choicePorts;
        public List<uint> _conditionPortIds;

        public string _speakerName;
        public string _dialogueText;

        public DialogueNodeData()
        {
            _choicePorts = new List<ChoicePortData>();
            _conditionPortIds = new List<uint>();
        }
    }

    [System.Serializable]
    public class AdvDialogueNodeData : DialogueNodeData
    {
        public Vector3 _cameraPos;
        public Vector3 _cameraRot;
        public float _lerpTime;

        public AdvDialogueNodeData()
        {

        }
    }

    [System.Serializable]
    public class CinematicDialogueNodeData : DialogueNodeData
    {
        public TimelineAsset _timelineAsset;

        public CinematicDialogueNodeData()
        {

        }
    }
}
