using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Asset", menuName = "Scriptable Objects/Dialogue Asset", order = 1)]
public class DialogueAsset : ScriptableObject
{
	[System.Serializable]
	struct DialogueFlag
	{
		public int flagId;
		public string flagName;
		public bool isSet;
	}

	[SerializeField]
	private string _dialogueId;
	[SerializeField]
	private DialogueFlag[] _progressionFlags;
}

public class DialogueLine
{
	[SerializeField]
	private int _speakerId;
	[SerializeField]
	private string _text;


	private string _speakerName;



	public int SpeakerID
	{
		get { return _speakerId; }
	}

	public string Text
	{
		get { return _text; }
	}
}
