using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
}
