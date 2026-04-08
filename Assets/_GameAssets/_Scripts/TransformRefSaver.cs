using UnityEngine;

public class TransformRefSaver : MonoBehaviour
{
    [SerializeField] private SO_TransformRef playerRef;
    [SerializeField] private Transform transformObject;

    private void Start()
    {
        if (this.playerRef != null && this.transformObject != null)
            this.playerRef.Value = this.transformObject;
    }
}