using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Kill state for Munch. On entry the monster teleports directly behind the player, plays a
    /// jumpscare animation and audio, and triggers the death camera sequence. After the configured
    /// timer duration elapses the player is killed.
    /// </summary>
    public class MunchDragState : MonsterStateWithTimer
    {
        [Header("Teleport")]
        [SerializeField] private float behindDistance = 1.5f;
        [SerializeField] private GameObject munchModel;

        /// <summary>
        /// Teleports Munch to a position directly behind the player, faces the monster toward them,
        /// plays the jumpscare audio and animation affordances, then triggers the camera death
        /// sequence and hides the Munch model while the cinematic plays.
        /// </summary>
        public override void OnStateEnter()
        {
            base.OnStateEnter();

            Transform player = this.controller.PlayerTarget;
            if (player != null)
            {
                this.controller.transform.position = player.position - player.forward * this.behindDistance;
                this.controller.transform.LookAt(player);
            }

            this.controller.PreviousState?.StopAffordances<AudioAffordance>();
            TriggerAffordances<AudioAffordance>();
            TriggerAffordances<AnimationAffordance>();

            CameraAnimationController cameraAnimation = Camera.main.GetComponentInChildren<CameraAnimationController>();
            if (cameraAnimation != null)
            {
                cameraAnimation.PlayMunchDeathAnimation();
                if (this.munchModel != null)
                    this.munchModel.SetActive(false); // hide model so it doesn't clip into the death camera
            }
            else
            {
                Debug.LogWarning("CameraAnimationController not found in children of main camera.");
            }
        }

        /// <summary>
        /// Called when the kill-delay timer expires. Delivers the killing blow to the player.
        /// </summary>
        protected override void OnTimerFinished()
        {
            KillPlayer();
        }
    }
}
