using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))] // Ensures the object always has an XRGrabInteractable component
public class FlashLight : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable handleInteractable;
    [SerializeField] private XRGrabInteractable crankInteractable;
    [SerializeField] private HingeJoint hinge;
    [SerializeField] private float[] triggerAngles;
    [SerializeField] private float lightPower;
    [SerializeField] private float maxCrank = 10;
    [SerializeField] private float crankDecayRate;
    [SerializeField] private float crankDecayTick;

    private float crankedPower;
    private float elapsedTime;
    private bool hasPower;
    private bool lowBattery;
    private bool[] triggered;
    
    public static event System.Action<float> OnCrankAngleTriggered;

    private void OnEnable()
    {
        crankInteractable.selectEntered.AddListener(OnGrab); 
        crankInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        crankInteractable.selectEntered.RemoveListener(OnGrab); 
        crankInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        handleInteractable.enabled = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        handleInteractable.enabled = true;
    }
    private void Start()
    {
        this.elapsedTime = 0;
        this.crankedPower = 0;
        this.lightPower = 0;
        this.hasPower = false;
        this.lowBattery = false;
        if (hinge == null) hinge = GetComponent<HingeJoint>(); 
        triggered = new bool[triggerAngles.Length];
    }

    private void Update()
    {
        this.elapsedTime += Time.deltaTime;

        // Only decay hunger once the tick interval has passed
        if (this.elapsedTime >= this.crankDecayTick)
        {
            this.elapsedTime = 0;

            if (this.crankedPower > 0)
            {
                this.crankedPower = Mathf.Max(this.crankedPower - this.crankDecayRate, 0);
            }
        }
        float angle = hinge.angle; for (int i = 0; i < triggerAngles.Length; i++) 
        {
            if (!triggered[i] && angle >= triggerAngles[i])
            {
                triggered[i] = true; OnCrankAngleTriggered?.Invoke(triggerAngles[i]);
            } 
        }
    }

    private void CrankFlashLight(float crankedAmount)
    {
        this.crankedPower =  Mathf.Max(this.crankedPower + crankedAmount, this.maxCrank);
    }
}
