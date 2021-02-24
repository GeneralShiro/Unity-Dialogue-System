using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CustomSystem.DialogueSystem
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject _dialoguePanel;
        public TextMeshProUGUI _speakerNameText;

        [Header("UI When No Choices Are Present")]
        public GameObject _dialogueNoChoicesPanel;
        public TextMeshProUGUI _dialogueNoChoicesText;
        public GameObject _dialogueContinueIcon;
        public GameObject _dialogueEndIcon;

        [Header("UI When Choices Are Present")]
        public GameObject _dialogueWithChoicesPanel;
        public TextMeshProUGUI _dialogueWithChoicesText;
        public ScrollRect _choicesScrollRect;
        public GameObject _dialogueChoicePrefab;
        public List<DialogueChoiceUI> _choices;


        public void SetDialogueText(DialogueNode node)
        {
            if (node == null)
            {
                return;
            }

            _speakerNameText.text = node.speakerName;
            bool hasChoices = (node.choices.Count != 0);

            if (!hasChoices)
            {
                _dialogueNoChoicesText.text = node.dialogueText;

                _dialogueContinueIcon.SetActive(node.childNodes.Count >= 1);
                _dialogueEndIcon.SetActive(node.childNodes.Count < 1);
            }
            else
            {
                _dialogueWithChoicesText.text = node.dialogueText;

                if (_choices == null)
                {
                    _choices = new List<DialogueChoiceUI>();
                }
                else    // clear out old dialogue choices
                {
                    foreach (DialogueChoiceUI choice in _choices)
                    {
                        Destroy(choice.gameObject);
                    }
                    _choices.Clear();
                }

                foreach (KeyValuePair<uint, DialogueNode.DialogueChoice> choiceEntry in node.choices)
                {
                    GameObject newChoicePanel = Instantiate(_dialogueChoicePrefab, _choicesScrollRect.content.transform);
                    DialogueChoiceUI choiceUI = newChoicePanel.GetComponent<DialogueChoiceUI>();

                    DialogueNode.DialogueChoice choice = choiceEntry.Value;
                    choiceUI.ChoiceText = choice.choiceText;
                    choiceUI.ChoiceId = choice.choiceId;

                    _choices.Add(choiceUI);
                }
            }

            _dialogueNoChoicesPanel.SetActive(!hasChoices);
            _dialogueWithChoicesPanel.SetActive(hasChoices);
        }

        public void OnDialoguePanelClick()
        {
            DialogueManager.dialogueManager.ContinueDialogue();
        }

        public void SetDialoguePanelVisibility(bool isVisible)
        {
            _dialoguePanel.SetActive(isVisible);
        }
    }
}
