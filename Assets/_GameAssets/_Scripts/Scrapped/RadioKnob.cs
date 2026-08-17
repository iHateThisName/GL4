using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RadioKnob : MonoBehaviour
{
    [Header("Knob Settings")]
    [SerializeField] private Transform knobTransform;
    [SerializeField] private int totalChannels = 5;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private float accumulatedAngle;
    private float lastInteractorAngle;
    private int currentChannel = -1;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.trackRotation = false;
        grabInteractable.trackPosition = false;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void Update()
    {
        if (!grabInteractable.isSelected)
            return;

        UpdateKnobRotation();
        UpdateChannel();
    }


    private void OnGrab(SelectEnterEventArgs args)
    {
        lastInteractorAngle = GetInteractorAngle(args.interactorObject.transform);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        lastInteractorAngle = 0f;
    }

    private void UpdateKnobRotation()
    {
        Transform interactor = grabInteractable.interactorsSelecting[0].transform;

        float currentInteractorAngle = GetInteractorAngle(interactor);
        float delta = Mathf.DeltaAngle(lastInteractorAngle, currentInteractorAngle);

        accumulatedAngle += delta;
        lastInteractorAngle = currentInteractorAngle;

        knobTransform.localRotation = Quaternion.Euler(0f, 0f, accumulatedAngle);
    }

    private void UpdateChannel()
    {
        int newChannel = Mathf.FloorToInt(Mathf.Abs(accumulatedAngle) / 45f) % totalChannels;

        if (newChannel == currentChannel)
            return;

        currentChannel = newChannel;
        OnChannelChanged(currentChannel);
    }

 
    private void OnChannelChanged(int channel)
    {
        Debug.Log($"Radio channel: {channel}");
    }

 

    private float GetInteractorAngle(Transform interactor)
    {
        Vector3 localDir = transform.InverseTransformDirection(interactor.forward);
        return Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
    }

    #region Getters
    public int GetCurrentChannel() => currentChannel;
    #endregion
}
