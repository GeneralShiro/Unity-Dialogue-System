using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
