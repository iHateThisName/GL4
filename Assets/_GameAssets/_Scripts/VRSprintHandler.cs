using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class VRSprintHandler : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 5.0f;
    [SerializeField] private float acceleration = 10.0f;

    [Header("Input Action")]
    [Tooltip("Map to: <XRController>{RightHand}/secondaryButton")]
    [SerializeField] private InputActionReference sprintAction;

    private DynamicMoveProvider moveProvider;
    private float targetSpeed;

    private void Awake()
    {
        moveProvider = GetComponent<DynamicMoveProvider>();

        // Start at your original walk speed
        targetSpeed = walkSpeed;
        if (moveProvider != null)
            moveProvider.moveSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        if (sprintAction != null)
        {
            sprintAction.action.Enable();
            sprintAction.action.performed += ctx => targetSpeed = sprintSpeed;
            sprintAction.action.canceled += ctx => targetSpeed = walkSpeed;
        }
    }

    private void OnDisable()
    {
        if (sprintAction != null)
        {
            sprintAction.action.performed -= ctx => targetSpeed = sprintSpeed;
            sprintAction.action.canceled -= ctx => targetSpeed = walkSpeed;
        }
    }

    private void Update()
    {
        if (moveProvider == null) return;

        // Smoothly shift between 2.5 and sprint speed
        if (!Mathf.Approximately(moveProvider.moveSpeed, targetSpeed))
        {
            moveProvider.moveSpeed = Mathf.MoveTowards(
                moveProvider.moveSpeed,
                targetSpeed,
                acceleration * Time.deltaTime
            );
        }
    }
}