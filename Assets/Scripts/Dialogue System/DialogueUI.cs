using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace CustomSystem.DialogueSystem
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject _dialoguePanel;
        public TextMeshProUGUI _speakerNameText;
        [Range(0.5f, 5.0f)]
        public float _textSpeed = 1.0f;
        public bool _isTextInstant;

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

        private bool _isFinalNode;
        private bool _hasChoices;
        private bool _isWritingText;
        private float _elapsedWritingTime;
        private string _dialogueString;
        private int _currentTextIndex;

        private void Update()
        {
            if (_isWritingText)
            {
                if (_elapsedWritingTime >= CharWritingTime)
                {
                    if (_currentTextIndex < _dialogueString.Length - 1)
                    {
                        TextDisplay.text = _dialogueString.Substring(0, _currentTextIndex + 1);
                        _currentTextIndex++;
                        _elapsedWritingTime = 0f;
                    }
                    else
                    {
                        EndDialogueWriting();
                    }
                }
                else
                {
                    _elapsedWritingTime += Time.deltaTime;
                }
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnDialogueClick();
            }
        }

        public void SetDialogueText(DialogueNode node)
        {
            if (node == null)
            {
                return;
            }

            _speakerNameText.text = node.speakerName;
            _hasChoices = (node.choices.Count != 0);
            TextDisplay.text = "";

            if (!_hasChoices)
            {
                _isFinalNode = (node.childNodes.Count < 1);
            }
            else
            {
                _choicesScrollRect.gameObject.SetActive(false);

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

            _dialogueString = node.dialogueText;
            _currentTextIndex = 0;
            _elapsedWritingTime = 0f;
            _isWritingText = true;

            _dialogueNoChoicesPanel.SetActive(!_hasChoices);
            _dialogueWithChoicesPanel.SetActive(_hasChoices);
        }

        public void OnDialogueClick()
        {
            if (_isWritingText)
            {
                EndDialogueWriting();
            }
            else if (!_hasChoices)
            {
                DialogueManager.dialogueManager.ContinueDialogue();
            }
        }

        public void SetDialoguePanelVisibility(bool isVisible)
        {
            _dialoguePanel.SetActive(isVisible);
        }

        public void EndDialogueWriting()
        {
            TextDisplay.text = _dialogueString;

            if (_hasChoices)
            {
                _choicesScrollRect.gameObject.SetActive(true);
            }
            else
            {
                _dialogueContinueIcon.SetActive(!_isFinalNode);
                _dialogueEndIcon.SetActive(_isFinalNode);
            }

            _isWritingText = false;
        }

        private float CharWritingTime
        {
            get { return _isTextInstant ? 0f : _textSpeed * 0.025f; }
        }

        public TextMeshProUGUI TextDisplay
        {
            get { return _hasChoices ? _dialogueWithChoicesText : _dialogueNoChoicesText; }
        }
    }
}
