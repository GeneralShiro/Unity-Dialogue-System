
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomSystem.DialogueSystem
{
    public class CinematicDialogueLine : DialogueControlAsset
    {
        public string _speakerName;
        public string _dialogueText;
        public bool _overrideWritingSpeed;
        public float _newWritingSpeed = 1f;
        public bool _waitForInputToContinue;
        public bool _hideUIAtEnd;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DialogueControlBehaviour>.Create(graph);

            var dialogueBehaviour = playable.GetBehaviour();
            dialogueBehaviour._speakerName = _speakerName;
            dialogueBehaviour._dialogueText = _dialogueText;
            dialogueBehaviour._overrideWritingSpeed = _overrideWritingSpeed;
            dialogueBehaviour._newWritingSpeed = _newWritingSpeed;
            dialogueBehaviour._waitForInputToContinue = _waitForInputToContinue;
            dialogueBehaviour._hideUIAtEnd = _hideUIAtEnd;

            return playable;
        }
    }
}