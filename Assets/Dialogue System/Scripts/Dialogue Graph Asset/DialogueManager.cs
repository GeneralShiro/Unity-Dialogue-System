﻿using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

namespace CustomSystem.DialogueSystem
{
    [RequireComponent(typeof(PlayableDirector))]
    public class DialogueManager : MonoBehaviour
    {
        protected static DialogueManager dialogueManager { get; set; }

        public CinemachineBrain _cmBrain;
        public CinemachineVirtualCamera _dialogueCamera1;
        public CinemachineVirtualCamera _dialogueCamera2;
        public PlayableDirector _timelineDirector;
        public DialogueUI _dialogueUI;

        private Camera _prevCamera;
        private DialogueTree _dialogueTree;
        private bool _isRunningDialogue;
        private bool _enableCam2NextFrame;


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

            if (_timelineDirector == null)
            {
                TryGetComponent<PlayableDirector>(out _timelineDirector);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _isRunningDialogue = _enableCam2NextFrame = false;

            _timelineDirector.stopped += (director) => { ContinueDialogue(); };
        }

        // Update is called once per frame
        void Update()
        {
            if (_isRunningDialogue)
            {
                if (_enableCam2NextFrame && !_dialogueCamera2.enabled && CurrentNode is AdvDialogueNode)
                {
                    //  turn on the second camera so the cinemachine brain can start transitioning from camera 1 to camera 2
                    _dialogueCamera2.enabled = true;
                    _enableCam2NextFrame = false;
                }
            }
        }

        public void LoadAsset(DialogueGraphAsset asset)
        {
            _dialogueTree = new DialogueTree(asset);
        }

        public void StartDialogue()
        {
            SetCurrentDialogue(_dialogueTree.StartNode);

            _dialogueUI.SetDialoguePanelVisibility(true);

            _dialogueCamera1.transform.position = Camera.main.transform.position;
            _dialogueCamera1.transform.rotation = Camera.main.transform.rotation;
            _dialogueCamera1.enabled = true;

            _isRunningDialogue = true;
        }

        private void SetCurrentDialogue(DialogueNode node)
        {
            if (!_dialogueTree.nodes.ContainsValue(node))
            {
                return;
            }

            // before moving on from the previous node, snap the camera to it's intended position/rotation first (if it was lerping)
            if (CurrentNode is AdvDialogueNode)
            {
                _enableCam2NextFrame = false;

                // reset cinemachine brain to 'snap' to second camera
                _cmBrain.enabled = false;
                _dialogueCamera1.enabled = _dialogueCamera2.enabled = false;

                // shift first virtual camera over to the final camera position (which is where the second virtual camera was)
                _dialogueCamera1.transform.position = _dialogueCamera2.transform.position;
                _dialogueCamera1.transform.rotation = _dialogueCamera2.transform.rotation;

                // turn the first virtual camera back, and turn the brain on too so it snaps the camera to the first virtual camera
                _cmBrain.enabled = true;
                _dialogueCamera1.enabled = true;
            }
            else if (CurrentNode is CinematicDialogueNode && _timelineDirector.state == PlayState.Playing)
            {
                _timelineDirector.Stop();
            }

            CurrentNode = node;

            if (node is CinematicDialogueNode)
            {
                CinematicDialogueNode castNode = node as CinematicDialogueNode;
                _timelineDirector.playableAsset = castNode.timelineAsset;
                _timelineDirector.Play();
            }
            else
            {
                _dialogueUI.SetDialogueText(node);

                if (node is AdvDialogueNode)
                {
                    AdvDialogueNode castNode = node as AdvDialogueNode;

                    // position & rotate camera 2 to the final camera position
                    _dialogueCamera2.transform.position = castNode.cameraWorldPos;
                    _dialogueCamera2.transform.rotation = Quaternion.Euler(castNode.cameraWorldRot);

                    // set the lerp time for the blend between camera 1 and camera 2
                    _cmBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Time = castNode.lerpTime;

                    // flag camera 2 to turn in the next Update frame
                    _enableCam2NextFrame = true;
                }
            }
        }

        public void ContinueDialogue(uint choiceId = 0)
        {
            //  Try to find another node to jump to; if we can't, end the dialogue
            if (CurrentNode.choices.Count > 0 || CurrentNode.childNodes.Count > 0)
            {
                //  If there are choice nodes present, check if the chosen choice node
                //  has any child nodes available
                if (CurrentNode.choices.Count > 0)
                {
                    DialogueNode.DialogueChoice choice = CurrentNode.choices[choiceId];

                    if (choice.childNodes.Count > 0)
                    {
                        DialogueNode nextNode = choice.FirstAvailableChildNode;

                        // if the chosen choice node has an available child, jump to it
                        if (nextNode != null)
                        {
                            SetCurrentDialogue(nextNode);
                        }
                        else
                        {
                            EndDialogue();
                        }
                    }
                    else
                    {
                        EndDialogue();
                    }
                }
                else
                {
                    //  If no choices are present, see if any of the child nodes
                    //  are valid based on their conditions
                    DialogueNode nextNode = CurrentNode.FirstAvailableChildNode;

                    if (nextNode != null)
                    {
                        SetCurrentDialogue(nextNode);
                    }
                    else
                    {
                        EndDialogue();
                    }
                }
            }
            else    // end dialogue
            {
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            _timelineDirector.Stop();
            _dialogueUI.SetDialoguePanelVisibility(false);
            _dialogueCamera1.enabled = _dialogueCamera2.enabled = false;
            _isRunningDialogue = false;
        }

        public void ContinueCinematicDialogue()
        {
            if (_timelineDirector.state == PlayState.Paused)
            {
                _timelineDirector.Resume();
            }
        }

        public static DialogueManager GetCurrentManager()
        {
            if (dialogueManager == null)
            {
                dialogueManager = FindObjectOfType<DialogueManager>();
            }

            return dialogueManager;
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

        public DialogueNode CurrentNode
        {
            get { return _dialogueTree.currentNode; }
            protected set { _dialogueTree.currentNode = value; }
        }
    }
}
