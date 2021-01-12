using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSpeakerComponent : MonoBehaviour
{
    [Serializable]
    public struct SpeakerID
	{
        public int id;
        public string name;
	}

    [SerializeField]
    private SpeakerID[] _speakerIds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetSpeakerName(int speakerID)
	{
        string name = "Name not found";

        for(int i = 0; i < _speakerIds.Length; i++)
		{
            if(_speakerIds[i].id == speakerID)
			{
                name = _speakerIds[i].name;
                break;
			}
		}

        return name;
	}
}
