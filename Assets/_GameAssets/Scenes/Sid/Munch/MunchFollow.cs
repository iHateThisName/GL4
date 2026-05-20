using UnityEngine;

public class MunchFollow : MonoBehaviour
{
    [Header("====== References ======")]
    [SerializeField] private SO_TransformRef playerRef;
    
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 5f; // smoothness
    
    private Transform targetTransform;
    [Gaskellgames.ReadOnly] private bool isFollowingTarget;

    private void OnEnable()
    {
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationChanged;
    }

    private void OnDisable()
    {
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationChanged;
    }

    private void Start() {
        if (this.playerRef != null)
            this.targetTransform = this.playerRef.Value.GetChild(0).GetChild(0);
        else
            this.targetTransform = GameObject.FindWithTag("Player").transform.root.transform;
        
        if (PlayerTemperatureSimulator.Instance.CurrentLocationType == PlayerTemperatureSimulator.EnumLocationType.Warm || PlayerTemperatureSimulator.Instance.CurrentLocationType == PlayerTemperatureSimulator.EnumLocationType.Normal)
            this.isFollowingTarget = true;
    }
    
    void FixedUpdate()
    {
        if (!this.isFollowingTarget) return;
        if (this.targetTransform == null) return;

        // Direction to target
        Vector3 direction = this.targetTransform.position - this.transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        // Create rotation where Z axis looks at target
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smooth rotation
        this. transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            targetRotation,
            this.rotationSpeed * Time.fixedDeltaTime
        );
    }
    
    private void HandleLocationChanged(PlayerTemperatureSimulator.EnumLocationType newLocation)
    {
        if (newLocation == PlayerTemperatureSimulator.EnumLocationType.Warm || newLocation == PlayerTemperatureSimulator.EnumLocationType.Normal) 
            this.isFollowingTarget = true;
        else
            this.isFollowingTarget = false;
    }
    
    public Transform CurrentTarget => this.targetTransform;
    
    public void SetTarget(Transform target) {
        this.targetTransform = target;
    }
}