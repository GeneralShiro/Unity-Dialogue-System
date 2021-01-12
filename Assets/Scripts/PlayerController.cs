using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float _moveSpeed = 1f;
    public float _rotationSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
	{
        Vector3 movement = new Vector3();

        if (Keyboard.current.wKey.isPressed)
        {
            movement.z = 1f;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            movement.z = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            movement.x = 1f;
        }
        else if (Keyboard.current.aKey.isPressed)
        {
            movement.x = -1f;
        }

        movement *= _moveSpeed * Time.deltaTime * 10;

        gameObject.transform.Translate(movement, Space.Self);
    }

    private void UpdateRotation()
	{
        gameObject.transform.Rotate(new Vector3(0f, Mouse.current.delta.x.ReadValue()*_rotationSpeed, 0f), Space.Self);
	}
}
