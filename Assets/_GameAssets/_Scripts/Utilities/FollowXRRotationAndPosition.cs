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
        float cameraY = this.parentTransform.eulerAngles.y;
        Quaternion bodyRotation = Quaternion.Euler(0f, cameraY, 0f);

        if (this.followPositionObj != null)
            this.transform.position = this.followPositionObj.position + bodyRotation * this.offset;

        if (this.followYRotation)
            this.transform.rotation = bodyRotation;
    }
}
