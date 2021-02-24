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

        public DialogueGraphAsset _testAsset;
        private DialogueTree _dialogueTree;
        private bool _isRunningDialogue;

        [Header("Dialogue UI")]
        public GameObject _dialoguePanel;
        public TextMeshProUGUI _speakerNameText;
        public TextMeshProUGUI _dialogueText;
        public GameObject _dialogueContinueIcon;
        public GameObject _dialogueEndIcon;


        private void OnEnable()
        {
            if (dialogueManager == null)
            {
                dialogueManager = this;
                DontDestroyOnLoad(gameObject);
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
            if(Keyboard.current.spaceKey.IsPressed() && !_isRunningDialogue){
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
            _dialoguePanel.SetActive(true);
            _isRunningDialogue = true;
        }

        private void SetCurrentDialogue(DialogueNode node)
        {
            if (!_dialogueTree.nodes.ContainsValue(node))
            {
                return;
            }

            _dialogueTree.currentNode = node;

            _speakerNameText.text = node.speakerName;
            _dialogueText.text = node.dialogueText;

            _dialogueContinueIcon.SetActive(node.childNodes.Count >= 1);
            _dialogueEndIcon.SetActive(node.childNodes.Count < 1);
        }

        public void ContinueDialogue()
        {
            if (_dialogueTree.currentNode.choices.Count > 0 || _dialogueTree.currentNode.childNodes.Count > 0)
            {
                //  TODO: handle moving to nodes via choices
                if (_dialogueTree.currentNode.choices.Count > 0)
                {

                }
                else
                {
                    //  TODO: handle moving to multiple nodes based on conditions

                    SetCurrentDialogue(_dialogueTree.currentNode.childNodes[0]);
                }
            }
            else    // end dialogue
            {
                _dialoguePanel.SetActive(false);
                _isRunningDialogue = false;
            }
        }
    }
}
