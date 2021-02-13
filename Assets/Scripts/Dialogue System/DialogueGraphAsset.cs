using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomGraphEditors.DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Custom Graph Asset/Dialogue Graph", order = 1)]
    public class DialogueGraphAsset : GraphAsset
    {
        public List<DialogueNodeData> dialogueNodeData;
        public List<AdvDialogueNodeData> advDialogueNodeData;
        public List<CinematicDialogueNodeData> cinematicDialogueNodeData;


        public DialogueGraphAsset()
        {
            nodeLinkData = new List<NodeLinkData>();

            booleanNodeData = new List<BooleanNodeData>();
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
        public string _speakerName;
        public string _dialogueText;
    }

    [System.Serializable]
    public class AdvDialogueNodeData : DialogueNodeData
    {
        public Vector3 _cameraPos;
        public Vector3 _cameraRot;
    }

    [System.Serializable]
    public class CinematicDialogueNodeData : DialogueNodeData
    {
        public TimelineAsset _timelineAsset;
    }
}
