using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Movement")]
    [SerializeField] public float MoveSpeed = 6f;
    [SerializeField] public float RotationSpeed = 10f;
    [SerializeField] public float Gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] public Transform CameraPivot;
    [SerializeField] public float MouseSensitivity = 2.5f;
    [SerializeField] public float MinY = -60f;
    [SerializeField] public float MaxY = 80f;



    /* =======================
     * Private Fields
     * ======================= */

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private bool canMove = true;


    /* =======================
     * Unity Lifecycle
     * ======================= */

    private void Awake()
    {
        this.controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!this.canMove)
        {
            return;
        }

        HandleMouseLook();
        HandleMovement();
    }


    /* =======================
     * Movement
     * ======================= */

    private void HandleMovement()
    {
        Vector3 inputDirection = GetMovementInput();

        if (inputDirection.magnitude > 0.1f)
        {
            RotateTowardsMovement(inputDirection);
        }

        ApplyGravity();

        Vector3 velocity = this.transform.forward * inputDirection.magnitude * this.MoveSpeed;
        velocity.y = this.verticalVelocity;

        this.controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 GetMovementInput()
    {
        Vector3 inputDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            inputDirection.z += 1f;
        if (Keyboard.current.sKey.isPressed)
            inputDirection.z -= 1f;
        if (Keyboard.current.aKey.isPressed)
            inputDirection.x -= 1f;
        if (Keyboard.current.dKey.isPressed)
            inputDirection.x += 1f;

        return inputDirection.normalized;
    }

    private void RotateTowardsMovement(Vector3 inputDirection)
    {
        Vector3 worldDirection = this.transform.TransformDirection(inputDirection);
        Quaternion targetRotation = Quaternion.LookRotation(worldDirection);

        this.transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            targetRotation,
            this.RotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (this.controller.isGrounded && this.verticalVelocity < 0f)
        {
            this.verticalVelocity = -2f;
        }

        this.verticalVelocity += this.Gravity * Time.deltaTime;
    }


    /* =======================
     * Mouse Look
     * ======================= */

    private void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * this.MouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * this.MouseSensitivity * Time.deltaTime;

        this.cameraPitch -= mouseY;
        this.cameraPitch = Mathf.Clamp(this.cameraPitch, this.MinY, this.MaxY);

        this.CameraPivot.localRotation = Quaternion.Euler(this.cameraPitch, 0f, 0f);
        this.transform.Rotate(Vector3.up * mouseX);
    }
}
