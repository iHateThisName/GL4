using UnityEngine;

/// <summary>
/// Controls the playback of camera-attached animations.
/// </summary>
public class CameraAnimationController : MonoBehaviour {

    [SerializeField] private GameObject munchDeathAnimation;

    /// <summary>
    /// Activates the GameObject containing the munch death animation.
    /// This GameObject is expected to possess an Animator component set to play automatically upon becoming active.
    /// </summary>
    [ContextMenu("Play Munch Death Animation")]
    public void PlayMunchDeathAnimation() {
        this.munchDeathAnimation.SetActive(true);
    }

}
