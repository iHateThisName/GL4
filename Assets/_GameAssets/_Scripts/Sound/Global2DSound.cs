using UnityEngine;
using UnityEngine.InputSystem;

public class Global2DSound : MonoBehaviour {
    private enum EnumMovementState { Idle = 0, Walking = 1, Running = 2 }
    private enum EnumWindState { Indoor = 0, Outdoor = 1 }

    [Header("===== FMod parameters =====")]
    [SerializeField] private string paramterMovementName = "Movement State";
    [SerializeField] private string paramterHungerName = "Hunger State";

    [Header("===== Input Actions =====")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference sprint;
    
    private readonly string globalParamterWindName = "Wind State";
    
    private FMODUnity.StudioEventEmitter emitter;
    private bool isSprinting = false;

    private void Awake() {
        this.emitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    private void OnEnable() {
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationTypeChanged;
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;

        InputAction moveAction = this.move.action; // Vector2 input.
        InputAction sprintAction = this.sprint.action; // Button input.

        if (moveAction != null) {
            moveAction.performed += HandleMoveInput;
            moveAction.canceled += HandleMoveInput;
        } else {
            Debug.LogWarning($"{nameof(Global2DSound)}: Move action is not assigned in the inspector.");
        }

        if (sprintAction != null) {
            sprintAction.performed += HandleSprintInput;
            sprintAction.canceled += HandleSprintInput;
        } else {
            Debug.LogWarning($"{nameof(Global2DSound)}: Sprint action is not assigned in the inspector.");
        }
    }

    private void OnDisable() {
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationTypeChanged;
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;

        InputAction moveAction = this.move.action;
        InputAction sprintAction = this.sprint.action;

        if (moveAction != null) {
            moveAction.performed -= HandleMoveInput;
            moveAction.canceled -= HandleMoveInput;
        }

        if (sprintAction != null) {
            sprintAction.performed -= HandleSprintInput;
            sprintAction.canceled -= HandleSprintInput;
        }
    }

    private void HandleSprintInput(InputAction.CallbackContext context) {
        if (context.performed) {
            this.isSprinting = true;
        } else if (context.canceled) {
            this.isSprinting = false;
        }
    }

    private void HandleMoveInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            if (this.isSprinting) {
                this.emitter.SetParameter(this.paramterMovementName, (int) EnumMovementState.Running);
            } else {
                this.emitter.SetParameter(this.paramterMovementName, (int)EnumMovementState.Walking);
            }
        } else if (context.phase == InputActionPhase.Canceled) {
            this.emitter.SetParameter(this.paramterMovementName, (int)EnumMovementState.Idle);
        }
    }

    private void HandleLocationTypeChanged(PlayerTemperatureSimulator.EnumLocationType type) {
        Debug.Log($"Location type changed to {type}");
        if (type == PlayerTemperatureSimulator.EnumLocationType.Normal || type == PlayerTemperatureSimulator.EnumLocationType.Warm) {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.globalParamterWindName, (float)EnumWindState.Indoor);
        } else if (type == PlayerTemperatureSimulator.EnumLocationType.Cold || type == PlayerTemperatureSimulator.EnumLocationType.Shack) {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.globalParamterWindName, (float)EnumWindState.Outdoor);
        }
    }
    
    private void OnHungerStateChanged(HungerSystem.EnumHungerState previous, HungerSystem.EnumHungerState current)
    {
        var fmodParamaterValue = (int)current;
        //this.emitter.SetParameter(this.paramterHungerName, fmodParamaterValue);
    }
    
    [ContextMenu("Movment Idle")] public void SetMovementIdle() => FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.paramterMovementName, (int)EnumMovementState.Idle);
    [ContextMenu("Movment Walking")] public void SetMovementWalking() => FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.paramterMovementName, (int)EnumMovementState.Walking);
    [ContextMenu("Movment Running")] public void SetMovementRunning() => FMODUnity.RuntimeManager.StudioSystem.setParameterByName(this.paramterMovementName, (int)EnumMovementState.Running);
}
