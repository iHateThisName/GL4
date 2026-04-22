using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;

public class WindStateTest : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private string parameterName = "WindChaseState";

    [SerializeField] private WindowController windowController;

    private float timer = 0f;
    private int counter = 0;
    private void Awake()
    {
        if (emitter == null)
            emitter = GetComponent<StudioEventEmitter>();
    }

    private void OnEnable()
    {
        if (this.windowController == null)
        {
            Debug.LogWarning($"{nameof(WindStateTest)}: windowController is not assigned in the inspector.");
            return;
        }

        this.windowController.windowJoint.OnLeverStateChanged += HandleWindowStateChanged;
    }

    private void OnDisable()
    {
        if (this.windowController != null) this.windowController.windowJoint.OnLeverStateChanged -= HandleWindowStateChanged;
    }

    public enum EnumWindMixer { Indoor = 0, Outdoor = 1 }
    public enum EnumDoorMixer { Open = 0, Closed = 1, LeaningOpen = 2, HoldOpen = 3 }
    [ContextMenu("Play Door Open, Value 0")] public void PlayDoorOpen() => SetParamAndLog(parameterName, (int)EnumDoorMixer.Open);
    [ContextMenu("Play Door Closed, Value 1")] public void PlayDoorClosed() => SetParamAndLog(parameterName, (int)EnumDoorMixer.Closed);
    [ContextMenu("Play Door Leaning Open, Value 2")] public void PlayDoorLeaningOpen() => SetParamAndLog(parameterName, (int)EnumDoorMixer.LeaningOpen);
    [ContextMenu("Play Door Hold Open, Value 3")] public void PlayDoorHoldOpen() => SetParamAndLog(parameterName, (int)EnumDoorMixer.HoldOpen);

    private void HandleWindowStateChanged(VRLever.EnumLeverState state)
    {
        if (emitter == null)
        {
            Debug.LogWarning($"{nameof(WindStateTest)}: emitter is null when window state changed.");
            return;
        }

        Debug.Log($"Window state changed to {state}. Emitter playing: {emitter.IsPlaying()}. Setting FMOD parameter '{parameterName}'.");

        switch (state)
        {
            case VRLever.EnumLeverState.Open:
                SetParamAndLog(parameterName, (int)EnumDoorMixer.Open);
                break;
            case VRLever.EnumLeverState.Closed:
                SetParamAndLog(parameterName, (int)EnumDoorMixer.Closed);
                break;
            case VRLever.EnumLeverState.LeaningOpen:

                if (this.windowController != null && this.windowController.IsGrabbed)
                {
                    SetParamAndLog(parameterName, (int)EnumDoorMixer.HoldOpen);
                }
                else
                {
                    SetParamAndLog(parameterName, (int)EnumDoorMixer.LeaningOpen);
                }
                break;
        }
    }

    // Helper wrapper to set parameter and then log diagnostic info
    private void SetParamAndLog(string name, float value)
    {
        try
        {
            // set on emitter (will cache if instance isn't active)
            emitter.Play();
            emitter.SetParameter(name, value);

            // attempt to read back parameter description and instance value
            DumpParameterInfo($"Set to {value}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception when setting FMOD parameter: {ex.Message}");
        }
    }

    // Diagnostic method: verifies the parameter exists on the EventDescription and reads the instance value (if available)
    private void DumpParameterInfo(string context = null)
    {
        if (emitter == null)
        {
            Debug.LogWarning("DumpParameterInfo: emitter is null.");
            return;
        }

        try
        {
            EventDescription desc = RuntimeManager.GetEventDescription(emitter.EventReference);
            if (!desc.isValid())
            {
                Debug.LogWarning("DumpParameterInfo: EventDescription invalid for emitter.EventReference.");
                return;
            }

            // List parameters and try to find the named parameter
            int paramCount;
            desc.getParameterDescriptionCount(out paramCount);
            bool found = false;
            for (int i = 0; i < paramCount; i++)
            {
                PARAMETER_DESCRIPTION pd;
                desc.getParameterDescriptionByIndex(i, out pd);
                Debug.Log($"FMOD Param[{i}] name:'{pd.name}' min:{pd.minimum} max:{pd.maximum} id:{pd.id}");
                if (pd.name == parameterName) found = true;
            }

            if (!found)
            {
                Debug.LogWarning($"DumpParameterInfo: Parameter '{parameterName}' not found on this event.");
            }

            // Read current value from the running instance if valid
            if (emitter.EventInstance.isValid())
            {
                float currentValue;
                FMOD.RESULT r = emitter.EventInstance.getParameterByName(parameterName, out currentValue);
                if (r == FMOD.RESULT.OK)
                {
                    Debug.Log($"DumpParameterInfo: Instance parameter '{parameterName}' = {currentValue} ({context})");
                }
                else
                {
                    Debug.LogWarning($"DumpParameterInfo: getParameterByName returned {r} for '{parameterName}'.");
                }
            }
            else
            {
                Debug.Log($"DumpParameterInfo: Event instance invalid (parameter set may be cached until instance is created or activated). Context: {context}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DumpParameterInfo exception: {ex.Message}");
        }
    }
}