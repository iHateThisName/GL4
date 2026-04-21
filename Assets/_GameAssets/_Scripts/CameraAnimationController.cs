using UnityEngine;

public class CameraAnimationController : MonoBehaviour
{

    [SerializeField] private GameObject munchDeathAnimation;

    [ContextMenu("Play Munch Death Animation")]
    public void PlayMunchDeathAnimation() {
        this.munchDeathAnimation.SetActive(true);
    }

}
