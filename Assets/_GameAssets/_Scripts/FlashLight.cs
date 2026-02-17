using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))] // Ensures the object always has an XRGrabInteractable component
public class FlashLight : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable handleInteractable;
    [SerializeField] private CrankRotationInteractable crankInteractable;
    [SerializeField] private float[] triggerAngles;
    [SerializeField] private float lightPower;
    [SerializeField] private float lightDecayRate;
    [SerializeField] private float lightDecayTick;
    [SerializeField] private int maxRotations;
    [SerializeField] private float maxLightPower;
    
    public float currentAngle { get; private set; }
    public int fullRotations { get; private set; }
    
    private float elapsedTime;
    private bool hasPower;
    private bool lowBattery;
    private const float LIGHT_MAGNITUDE = 2;

    private void OnEnable()
    {
        if (crankInteractable != null) crankInteractable.OnCrank += OnCrankRotated;
    }

    private void OnDisable()
    {
        if (crankInteractable != null) crankInteractable.OnCrank -= OnCrankRotated;
    }
    
    private void Start()
    {
        this.elapsedTime = 0;
        //this.lightPower = 0;
        this.hasPower = false;
        this.lowBattery = false;
    }

    private void Update()
    {
        this.elapsedTime += Time.deltaTime;

        // Only decay hunger once the tick interval has passed
        if (this.elapsedTime >= this.lightDecayTick)
        {
            this.elapsedTime = 0;

            if (this.lightPower > 0)
            {
                this.lightPower = Mathf.Max(this.lightPower - this.lightDecayRate, 0);
            }
        }
    }
    
    void OnCrankRotated(float delta)
    {
        if (this.fullRotations >= this.maxRotations) return;
        
        this.currentAngle += delta;
        
        // Count full rotations
        while (this.currentAngle >= 360f)
        {
            this.currentAngle -= 360f;
            this.fullRotations++;
        }

        while (this.currentAngle <= -360f)
        {
            this.currentAngle += 360f;
            this.fullRotations--;
        }
        
        lightPower = Mathf.Clamp(this.lightPower + (this.fullRotations * LIGHT_MAGNITUDE),0, this.maxLightPower);
    }
}
