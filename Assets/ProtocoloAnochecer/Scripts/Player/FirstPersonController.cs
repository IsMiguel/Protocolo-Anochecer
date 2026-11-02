using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonController : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
    [SerializeField, Min(0.1f)] private float sprintSpeed = 7.5f;
    [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private float cameraPitch;
    private float verticalVelocity;
    private bool cursorLocked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            SetCursorLocked(!cursorLocked);
        else if (mouse != null && !cursorLocked && mouse.leftButton.wasPressedThisFrame)
            SetCursorLocked(true);

        if (mouse != null && cursorLocked)
            Look(mouse);

        Vector2 moveInput = Vector2.zero;
        bool sprinting = false;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;
            sprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }

        Move(Vector2.ClampMagnitude(moveInput, 1f), sprinting);
    }

    private void Look(Mouse mouse)
    {
        Vector2 lookInput = mouse.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(Vector3.up, lookInput.x, Space.World);

        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -85f, 85f);
        if (viewCamera != null)
            viewCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move(Vector2 moveInput, bool sprinting)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        float speed = sprinting ? sprintSpeed : moveSpeed;
        Vector3 horizontalMovement =
            (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;
        Vector3 movement = horizontalMovement + Vector3.up * verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void OnDisable()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
