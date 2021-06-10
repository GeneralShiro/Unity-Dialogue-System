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
        [Range(0.1f, 3.0f)]
        public float _textSpeed = 1.0f;
        public bool _isTextInstant;

        [Header("UI When No Choices Are Present")]
        public GameObject _dialogueNoChoicesPanel;
        public TextMeshProUGUI _dialogueNoChoicesText;
        public TextMeshProUGUI _dialogueNoChoicesTextSizer;
        public GameObject _dialogueContinueIcon;
        public GameObject _dialogueEndIcon;

        [Header("UI When Choices Are Present")]
        public GameObject _dialogueWithChoicesPanel;
        public TextMeshProUGUI _dialogueWithChoicesText;
        public TextMeshProUGUI _dialogueWithChoicesTextSizer;
        public ScrollRect _choicesScrollRect;
        public GameObject _dialogueChoicePrefab;
        public List<DialogueChoiceUI> _choices;

        private Animator _animator;
        private bool _isFinalNode;
        private bool _hasChoices;
        private bool _isWritingText;
        private float _elapsedWritingTime;
        private string _dialogueString;


        private void Update()
        {
            if (DialogueManager.IsRunningDialogue)
            {
                ResizeDialogueTextRect();
            }

            if (_isWritingText && UIAnimator.GetCurrentAnimatorStateInfo(0).IsName("Panel Visible"))
            {
                string currWrittenText = "";
                bool isFinished = GetCurrentWrittenDialogue(_elapsedWritingTime, out currWrittenText);
                TextDisplay.text = currWrittenText;

                if (isFinished)
                {
                    EndDialogueWriting();
                }
                else
                {
                    _elapsedWritingTime += Time.deltaTime;
                }
            }
        }

        public void SetDialogueText(DialogueNode node)
        {
            if (node == null)
            {
                return;
            }

            _speakerNameText.text = node.speakerName;
            _hasChoices = node.HasAvailableChoices;
            TextDisplay.text = "";

            TextSizer.text = node.dialogueText;

            if (!_hasChoices)
            {
                _isFinalNode = (node.FirstAvailableChildNode == null);
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
                    if (choiceEntry.Value.FirstAvailableChildNode != null)
                    {
                        GameObject newChoicePanel = Instantiate(_dialogueChoicePrefab, _choicesScrollRect.content.transform);
                        DialogueChoiceUI choiceUI = newChoicePanel.GetComponent<DialogueChoiceUI>();

                        DialogueNode.DialogueChoice choice = choiceEntry.Value;
                        choiceUI.ChoiceText = choice.choiceText;
                        choiceUI.ChoiceId = choice.choiceId;

                        _choices.Add(choiceUI);
                    }
                }
            }

            UIAnimator.SetBool("IsContinueEndIconVisible", false);
            _dialogueContinueIcon.SetActive(false);
            _dialogueEndIcon.SetActive(false);

            _dialogueString = node.dialogueText;
            _elapsedWritingTime = 0f;
            _isWritingText = true;

            _dialogueNoChoicesPanel.SetActive(!_hasChoices);
            _dialogueWithChoicesPanel.SetActive(_hasChoices);
        }

        /// <summary>
        /// Intended for use with Cinematic Dialogue Nodes; sets dialogue text without DialogueNode to pull data from.
        /// </summary>
        public bool SetDialogueText(string speaker, string dialogueText, float elapsedWritingTime, float overrideSpeed = -1f)
        {
            _speakerNameText.text = speaker;
            _dialogueString = dialogueText;
            TextSizer.text = dialogueText;

            _hasChoices = false;
            _dialogueNoChoicesPanel.SetActive(true);
            _dialogueWithChoicesPanel.SetActive(false);

            string text;
            bool isCompleted = GetCurrentWrittenDialogue(elapsedWritingTime, out text, overrideSpeed);
            TextDisplay.text = text;

            ResizeDialogueTextRect();

            return isCompleted;
        }

        public void ContinueDialogue()
        {
            if (DialogueManager.GetCurrentManager().CurrentNode is CinematicDialogueNode)
            {
                DialogueManager.GetCurrentManager().ContinueCinematicDialogue();
            }
            else
            {
                if (_isWritingText)
                {
                    EndDialogueWriting();
                }
                else if (!_hasChoices)
                {
                    DialogueManager.GetCurrentManager().ContinueDialogue();
                }
            }
        }

        public void SetDialoguePanelVisibility(bool isVisible)
        {
            UIAnimator.SetBool("IsDialoguePanelVisible", isVisible);
            UIAnimator.SetBool("IsContinueEndIconVisible", false);

            _dialogueContinueIcon.SetActive(false);
            _dialogueEndIcon.SetActive(false);
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
                UIAnimator.SetBool("IsContinueEndIconVisible", true);
                _dialogueContinueIcon.SetActive(!_isFinalNode);
                _dialogueEndIcon.SetActive(_isFinalNode);
            }

            _isWritingText = false;
        }

        private void ResizeDialogueTextRect()
        {
            TextDisplay.rectTransform.anchorMax = TextSizer.rectTransform.anchorMax;
            TextDisplay.rectTransform.anchorMin = TextSizer.rectTransform.anchorMin;
            TextDisplay.rectTransform.anchoredPosition = TextSizer.rectTransform.anchoredPosition;
            TextDisplay.rectTransform.sizeDelta = TextSizer.rectTransform.sizeDelta;
        }

        /// <summary>
        /// Gets the substring of the current dialogue text based on how much time has passed. 
        /// Returns a boolean for whether or not the dialogue text is completely written.
        /// </summary>
        public bool GetCurrentWrittenDialogue(float elapsedWritingTime, out string outText, float overrideSpeed = -1f)
        {
            if (_dialogueString == null)
            {
                outText = "";
                return false;
            }

            int lastIndex = _dialogueString.Length;

            if (!_isTextInstant)
            {
                float speed = (overrideSpeed > 0f) ? (0.025f / overrideSpeed) : CharWritingInterval;
                float stringProgress = elapsedWritingTime / speed;
                lastIndex = (int)stringProgress;

                if (lastIndex > _dialogueString.Length)
                {
                    lastIndex = _dialogueString.Length;
                }
            }

            outText = _dialogueString.Substring(0, lastIndex);

            return (lastIndex == _dialogueString.Length);
        }

        public float DialogueLineDuration
        {
            get
            {
                if (_dialogueString != null)
                {
                    return (_dialogueString.Length * CharWritingInterval);
                }

                return 0f;
            }
        }

        private float CharWritingInterval
        {
            get { return _isTextInstant ? 0f : 0.025f / _textSpeed; }
        }

        public TextMeshProUGUI TextDisplay
        {
            get { return _hasChoices ? _dialogueWithChoicesText : _dialogueNoChoicesText; }
        }

        public TextMeshProUGUI TextSizer
        {
            get { return _hasChoices ? _dialogueWithChoicesTextSizer : _dialogueNoChoicesTextSizer; }
        }

        private Animator UIAnimator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }

                return _animator;
            }
        }
    }
}
