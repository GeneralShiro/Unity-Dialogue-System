using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;

namespace CustomGraphEditors.DialogueSystem
{
	[CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Custom Graph Asset/Dialogue Graph", order = 1)]
	public class DialogueGraphAsset : GraphAsset
	{
		public DialogueGraphAsset()
		{
			nodeData = new List<NodeData>();
			nodeLinkData = new List<NodeLinkData>();
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
