using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public Transform cameraPivot;
    public float mouseSensitivity = 2.5f;
    public float minY = -60f;
    public float maxY = 80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 inputDir = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            inputDir.z += 1f;
        if (Keyboard.current.sKey.isPressed)
            inputDir.z -= 1f;
        if (Keyboard.current.aKey.isPressed)
            inputDir.x -= 1f;
        if (Keyboard.current.dKey.isPressed)
            inputDir.x += 1f;

        inputDir = inputDir.normalized;

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 moveDir = transform.TransformDirection(inputDir);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = transform.forward * inputDir.magnitude * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minY, maxY);

        cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
