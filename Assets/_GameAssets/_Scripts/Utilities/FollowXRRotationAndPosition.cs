using UnityEngine;

public class FollowXRRotationAndPosition : MonoBehaviour
{
    [SerializeField] private bool followCameraY = true;
    [SerializeField] private Transform followPosition;
    
    private Transform parentTransform;

    private void Start()
    {
        this.parentTransform = Camera.main?.transform;
    }

    private void LateUpdate()
    {
        if (this.followPosition != null)
            this.transform.position = this.followPosition.position;

        if (this.followCameraY)
        {
            float cameraY = this.parentTransform.eulerAngles.y;
            this.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);
        }
    }
}
