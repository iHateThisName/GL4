using UnityEngine;

public class FollowXRRotationAndPosition : MonoBehaviour
{
    [SerializeField] private bool followCameraY = true;
    [SerializeField] private Transform followPosition;
    [SerializeField] private Vector3 offset = new Vector3(-0.2f, 0.9f, 0.25f);
    
    private Transform parentTransform;

    private void Start()
    {
        this.parentTransform = Camera.main?.transform;
    }

    private void LateUpdate()
    {
        if (this.followPosition != null)
            this.transform.position = this.followPosition.position + this.offset;

        if (this.followCameraY)
        {
            float cameraY = this.parentTransform.eulerAngles.y;
            this.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);
        }
    }
}
