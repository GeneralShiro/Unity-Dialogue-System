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

        public Camera _dialogueCamera;
        public PlayableDirector _timelineDirector;
        public DialogueUI _dialogueUI;

        public DialogueGraphAsset _testAsset;
        private Camera _prevCamera;
        private DialogueTree _dialogueTree;
        private bool _isRunningDialogue;
        private bool _isLerpingCamera;
        private float _cameraElapsedLerpTime;
        private float _cameraTotalLerpTime;
        private Vector3 _cameraOriginalPos;
        private Vector3 _cameraTargetPos;
        private Quaternion _cameraOriginalRot;
        private Quaternion _cameraTargetRot;


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
            if (_dialogueCamera == null)
            {
                TryGetComponent<Camera>(out _dialogueCamera);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadAsset(_testAsset);      // use this for testing; remove later
            _isRunningDialogue = _isLerpingCamera = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isRunningDialogue)
            {
                if (Keyboard.current.spaceKey.IsPressed())
                {
                    StartDialogue();
                }
            }
            else
            {
                if (_isLerpingCamera)
                {
                    if (_cameraElapsedLerpTime < _cameraTotalLerpTime)
                    {
                        float lerpTime = _cameraElapsedLerpTime / _cameraTotalLerpTime;

                        _dialogueCamera.transform.position = SmoothStepVector3(_cameraOriginalPos, _cameraTargetPos, lerpTime);
                        _dialogueCamera.transform.rotation = Quaternion.Slerp(_cameraOriginalRot, _cameraTargetRot, lerpTime);
                    }
                    else
                    {
                        _dialogueCamera.transform.position = _cameraTargetPos;
                        _dialogueCamera.transform.rotation = _cameraTargetRot;
                        _isLerpingCamera = false;
                    }

                    _cameraElapsedLerpTime += Time.deltaTime;
                }
            }
        }

        public void LoadAsset(DialogueGraphAsset asset)
        {
            _dialogueTree = new DialogueTree(asset);
        }

        public void StartDialogue()
        {
            SetCurrentDialogue(_dialogueTree.startNode);

            _dialogueUI.SetDialoguePanelVisibility(true);

            _prevCamera = Camera.main;
            _dialogueCamera.transform.position = _prevCamera.transform.position;
            _dialogueCamera.transform.rotation = _prevCamera.transform.rotation;
            _prevCamera.enabled = false;
            _dialogueCamera.enabled = true;

            _isRunningDialogue = true;
        }

        private void SetCurrentDialogue(DialogueNode node)
        {
            if (!_dialogueTree.nodes.ContainsValue(node))
            {
                return;
            }

            // before moving on from the previous node, snap the camera to it's intended position/rotation first (if it was lerping)
            if (_dialogueTree.currentNode is AdvDialogueNode && _isLerpingCamera)
            {
                _isLerpingCamera = false;
                _dialogueCamera.transform.SetPositionAndRotation(_cameraTargetPos, _cameraTargetRot);
            }
            
            _dialogueTree.currentNode = node;

            _dialogueUI.SetDialogueText(node);

            if (node is AdvDialogueNode)
            {
                AdvDialogueNode castNode = node as AdvDialogueNode;

                _cameraOriginalPos = _dialogueCamera.transform.position;
                _cameraOriginalRot = _dialogueCamera.transform.rotation;

                _cameraTargetPos = castNode.cameraWorldPos;
                _cameraTargetRot = Quaternion.Euler(castNode.cameraWorldRot);

                _cameraTotalLerpTime = castNode.lerpTime;
                _cameraElapsedLerpTime = 0f;
                _isLerpingCamera = true;
            }
        }

        public void ContinueDialogue(uint choiceId = 0)
        {
            if (_dialogueTree.currentNode.choices.Count > 0 || _dialogueTree.currentNode.childNodes.Count > 0)
            {
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
                        EndDialogue();
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
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            _dialogueUI.SetDialoguePanelVisibility(false);

            _dialogueCamera.enabled = false;
            _prevCamera.enabled = true;
            _dialogueCamera.transform.position = Vector3.zero;

            _isRunningDialogue = false;
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

        public Vector3 SmoothStepVector3(Vector3 start, Vector3 end, float time)
        {
            return new Vector3(
                Mathf.SmoothStep(start.x, end.x, time),
                Mathf.SmoothStep(start.y, end.y, time),
                Mathf.SmoothStep(start.z, end.z, time)
            );
        }
    }
}
