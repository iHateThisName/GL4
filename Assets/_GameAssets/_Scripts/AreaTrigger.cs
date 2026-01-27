using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    [SerializeField] string tagInQuestion;
    private bool isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagInQuestion))
        {
            isInside = true;
        }
    }

    public bool IsInside()
    {
        return isInside;
    }

    public void SetInside(bool isInside)
    {
        this.isInside = isInside;
    }
}
