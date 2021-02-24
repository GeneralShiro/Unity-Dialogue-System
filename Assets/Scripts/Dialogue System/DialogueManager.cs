using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using TMPro;

namespace CustomSystem.DialogueSystem
{
    [RequireComponent(
        typeof(Camera),
        typeof(PlayableDirector)
        )]
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager dialogueManager { get; protected set; }

        public Camera _camera;
        public PlayableDirector _timelineDirector;
        public DialogueUI _dialogueUI;

        public DialogueGraphAsset _testAsset;
        private DialogueTree _dialogueTree;
        private bool _isRunningDialogue;


        private void OnEnable()
        {
            if (dialogueManager == null)
            {
                dialogueManager = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            if (_timelineDirector == null)
            {
                TryGetComponent<PlayableDirector>(out _timelineDirector);
            }
            if (_camera == null)
            {
                TryGetComponent<Camera>(out _camera);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadAsset(_testAsset);      // use this for testing; remove later
            _isRunningDialogue = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.spaceKey.IsPressed() && !_isRunningDialogue)
            {
                OpenDialoguePanel();
            }
        }

        public void LoadAsset(DialogueGraphAsset asset)
        {
            _dialogueTree = new DialogueTree(asset);
        }

        public void OpenDialoguePanel()
        {
            SetCurrentDialogue(_dialogueTree.startNode);
            _dialogueUI.SetDialoguePanelVisibility(true);
            _isRunningDialogue = true;
        }

        private void SetCurrentDialogue(DialogueNode node)
        {
            if (!_dialogueTree.nodes.ContainsValue(node))
            {
                return;
            }

            _dialogueTree.currentNode = node;

            _dialogueUI.SetDialogueText(node);
        }

        public void ContinueDialogue(uint choiceId = 0)
        {
            if (_dialogueTree.currentNode.choices.Count > 0 || _dialogueTree.currentNode.childNodes.Count > 0)
            {
                //  TODO: handle moving to nodes via choices
                if (_dialogueTree.currentNode.choices.Count > 0)
                {
                    DialogueNode.DialogueChoice choice = _dialogueTree.currentNode.choices[choiceId];

                    if (choice.childNodes.Count > 0)
                    {
                        //  TODO: handle moving to multiple nodes based on conditions
                        SetCurrentDialogue(choice.childNodes[0]);
                    }
                    else
                    {
                        _dialogueUI.SetDialoguePanelVisibility(false);
                        _isRunningDialogue = false;
                    }
                }
                else
                {
                    //  TODO: handle moving to multiple nodes based on conditions
                    SetCurrentDialogue(_dialogueTree.currentNode.childNodes[0]);
                }
            }
            else    // end dialogue
            {
                _dialogueUI.SetDialoguePanelVisibility(false);
                _isRunningDialogue = false;
            }
        }

        public static bool IsRunningDialogue
        {
            get
            {
                bool ret = false;

                if (dialogueManager != null)
                {
                    ret = dialogueManager._isRunningDialogue;
                }

                return ret;
            }
        }
    }
}
