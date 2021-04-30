using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CustomSystem.DialogueSystem;

public class PlayerController : MonoBehaviour
{
    public float _moveSpeed = 1f;
    public float _rotationSpeed = 1f;

    private Vector3 _prevMousePosition;


    // Update is called once per frame
    void Update()
    {
        if (IsReceivingInput)
        {
            if (_prevMousePosition == null)
            {
                _prevMousePosition = Input.mousePosition;
            }

            UpdatePosition();
            UpdateRotation();

            _prevMousePosition = Input.mousePosition;
        }
    }

    private void UpdatePosition()
    {
        Vector3 movement = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            movement.z = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement.z = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movement.x = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movement.x = -1f;
        }

        movement *= _moveSpeed * Time.deltaTime * 10;

        gameObject.transform.Translate(movement, Space.Self);
    }

    private void UpdateRotation()
    {
        Vector3 deltaPos = _prevMousePosition - Input.mousePosition;

        if (Mathf.Abs(deltaPos.x) >= 1f)
        {
            gameObject.transform.Rotate(new Vector3(0f, (deltaPos.x * _rotationSpeed * -.2f), 0f), Space.Self);
        }
    }

    public bool IsReceivingInput
    {
        get
        {
            return !DialogueManager.IsRunningDialogue;
        }
    }
}
