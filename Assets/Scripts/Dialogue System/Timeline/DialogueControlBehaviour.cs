using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;

namespace CustomSystem.DialogueSystem
{
    public class DialogueControlBehaviour : PlayableBehaviour
    {
        public string _speakerName;
        public string _dialogueText;
        public bool _hideUIAtEnd;
        

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (DialogueManager.GetCurrentManager() != null)
            {
                if (PlayableExtensions.IsDone(playable))
                {
                    Debug.Log("clip finished");
                }
                else
                {
                    double currentTime = PlayableExtensions.GetTime(playable);
                    DialogueManager.GetCurrentManager()._dialogueUI.SetDialogueText(_speakerName, _dialogueText, (float)currentTime);
                }
            }
        }
    }
}