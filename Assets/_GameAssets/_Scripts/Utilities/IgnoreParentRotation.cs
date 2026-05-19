using UnityEngine;

public class IgnoreParentRotation : MonoBehaviour
{
    [SerializeField] private bool ignoreX, ignoreY, ignoreZ;
    
    private void Update()
    {
        Vector3 worldEuler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(
            ignoreX ? 0 : worldEuler.x,
            ignoreY ? 0 : worldEuler.y,
            ignoreZ ? 0 : worldEuler.z
        );
    }
}