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
            dialogueNodeData = new List<DialogueNodeData>();
            advDialogueNodeData = new List<AdvDialogueNodeData>();
            cinematicDialogueNodeData = new List<CinematicDialogueNodeData>();
        }

        public override NodeData GetNodeDataByGuid(uint guid)
        {
            NodeData data = base.GetNodeDataByGuid(guid);

            if (data == null)
            {
                for (int i = 0; i < dialogueNodeData.Count; i++)
                {
                    if (guid == dialogueNodeData[i]._nodeGuid)
                    {
                        return dialogueNodeData[i];
                    }
                }

                for (int i = 0; i < advDialogueNodeData.Count; i++)
                {
                    if (guid == advDialogueNodeData[i]._nodeGuid)
                    {
                        return advDialogueNodeData[i];
                    }
                }

                for (int i = 0; i < cinematicDialogueNodeData.Count; i++)
                {
                    if (guid == cinematicDialogueNodeData[i]._nodeGuid)
                    {
                        return cinematicDialogueNodeData[i];
                    }
                }

                return null;
            }
            else
            {
                return data;
            }
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

        public bool _isCollapsed;

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

        public AdvDialogueNodeData() { }
    }

    [System.Serializable]
    public class CinematicDialogueNodeData : DialogueNodeData
    {
        public TimelineAsset _timelineAsset;

        public CinematicDialogueNodeData() { }
    }
}
