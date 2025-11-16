using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float gravity;

    CharacterController controller;
    Vector3 moveVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = StaticInputManager.input.Player.Move.ReadValue<Vector2>();

        Vector3 adjustedInput = GetCameraRelativeInput(moveInput);

        if (adjustedInput != Vector3.zero)
            transform.forward = adjustedInput.normalized;

        moveVelocity = new Vector3(adjustedInput.x * moveSpeed, moveVelocity.y, adjustedInput.z * moveSpeed);

        moveVelocity.y += gravity * Time.deltaTime;

        controller.Move((moveVelocity) * Time.deltaTime);
    }

    Vector3 GetCameraRelativeInput(Vector2 input)
    {
        Transform cam = Camera.main.transform;

        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        return input.x * camRight + input.y * camForward;
    }
}
