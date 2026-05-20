using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
    [SerializeField] private bool ignoreX, ignoreY, ignoreZ;
    [Range(0, 2)] [Tooltip("0 = Update, 1 = FixedUpdate, 2 = LateUpdate")]
    [SerializeField] private int typeOfUpdate;

    private Quaternion _lockedRotation;

    private void Start()
    {
        _lockedRotation = transform.rotation;
    }

    private void ApplyIgnore()
    {
        Quaternion current = transform.rotation;
        transform.rotation = Quaternion.Euler(
            ignoreX ? _lockedRotation.eulerAngles.x : current.eulerAngles.x,
            ignoreY ? _lockedRotation.eulerAngles.y : current.eulerAngles.y,
            ignoreZ ? _lockedRotation.eulerAngles.z : current.eulerAngles.z
        );
    }

    private void Update()
    {
        if (typeOfUpdate != 0) return;
        ApplyIgnore();
    }

    private void FixedUpdate()
    {
        if (typeOfUpdate != 1) return;
        ApplyIgnore();
    }

    private void LateUpdate()
    {
        if (typeOfUpdate != 2) return;
        ApplyIgnore();
    }
}