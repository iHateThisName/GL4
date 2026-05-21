using UnityEngine;

public class FollowXRRotationAndPosition : MonoBehaviour
{
    [SerializeField] private bool followYRotation = true;
    [SerializeField] private Transform followPositionObj;
    [SerializeField] private Transform followRotationObj;
    [SerializeField] private Vector3 offset = new Vector3(-0.2f, 0.9f, 0.25f);
    
    private Transform parentTransform;

    private void Start()
    {
        this.parentTransform = this.followRotationObj == null ? Camera.main?.transform : this.followRotationObj;
    }

    private void LateUpdate()
    {
        if (this.followPositionObj != null)
            this.transform.position = this.followPositionObj.position + this.offset;

        if (this.followYRotation)
        {
            float cameraY = this.parentTransform.eulerAngles.y;
            this.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);
        }
    }
}
