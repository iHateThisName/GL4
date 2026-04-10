using UnityEngine;

public class TransformRefSaver : MonoBehaviour
{
    [SerializeField] private SO_TransformRef transformRef;
    [SerializeField] private Transform transformObject;

    private void Awake()
    {
        if (this.transformRef != null && this.transformObject != null)
            this.transformRef.Value = this.transformObject;
    }
}