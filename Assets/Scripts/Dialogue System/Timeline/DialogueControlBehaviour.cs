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
        public bool _overrideWritingSpeed;
        public float _newWritingSpeed;
        public bool _waitForInputToContinue;
        public bool _hideUIAtEnd;


        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if ((info.effectivePlayState == PlayState.Paused && (playable.GetTime() + info.deltaTime) > playable.GetDuration())
            || playable.GetGraph().GetRootPlayable(0).IsDone())
            {
                if (_waitForInputToContinue)
                {
                    playable.GetGraph().Stop();
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (DialogueManager.GetCurrentManager() != null)
            {
                double currentTime = PlayableExtensions.GetTime(playable);
                float newSpeed = _overrideWritingSpeed ? _newWritingSpeed : -1f;
                DialogueManager.GetCurrentManager()._dialogueUI.SetDialogueText(_speakerName, _dialogueText, (float)currentTime, newSpeed);
            }
        }
    }
}