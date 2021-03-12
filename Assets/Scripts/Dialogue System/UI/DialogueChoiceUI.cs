using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace CustomSystem.DialogueSystem
{
    public class DialogueChoiceUI : MonoBehaviour
    {
        public TextMeshProUGUI _choiceText;
        private uint _choiceId;

        public void OnChoiceClick()
        {
            DialogueManager.GetCurrentManager().ContinueDialogue(_choiceId);
        }

        public string ChoiceText
        {
            get { return _choiceText.text; }
            set { _choiceText.text = value; }
        }

        public uint ChoiceId
        {
            get { return _choiceId; }
            set { _choiceId = value; }
        }
    }
}
