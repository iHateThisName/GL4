using UnityEngine;


public class RadioKnob : MonoBehaviour
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Knob Settings")]
    [SerializeField] private Transform knobTransform;
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    [Header("Channel Settings")]
    [SerializeField] private int totalChannels = 5;

    /* =======================
     * Private Fields
     * ======================= */

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private float currentAngle;
    private int currentChannel = -1;

    /* =======================
     * Unity Lifecycle
     * ======================= */

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // We handle rotation manually
        grabInteractable.trackRotation = false;
    }

    private void Update()
    {
        if (!grabInteractable.isSelected)
            return;

        UpdateKnobRotation();
        UpdateChannel();
    }

    /* =======================
     * Knob Logic
     * ======================= */

    private void UpdateKnobRotation()
    {
        float rawAngle = grabInteractable.transform.localEulerAngles.z;
        currentAngle = NormalizeAngle(rawAngle);
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        knobTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    private void UpdateChannel()
    {
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
        int newChannel = Mathf.RoundToInt(t * (totalChannels - 1));

        if (newChannel == currentChannel)
            return;

        currentChannel = newChannel;
        OnChannelChanged(currentChannel);
    }

    /* =======================
     * Channel Events
     * ======================= */

    private void OnChannelChanged(int channel)
    {
        Debug.Log($"Radio channel changed to: {channel}");
        // Hook radio audio / logic here
    }

    /* =======================
     * Helpers
     * ======================= */

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
