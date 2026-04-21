using UnityEngine;

namespace MonsterSystem
{
    /// Munch teleports behind the player, jumpscares, and kills them after the timer duration.
    /// Set the duration field on MonsterStateWithTimer to control the kill delay.
    public class MunchDragState : MonsterStateWithTimer
    {
        [Header("Teleport")]
        [SerializeField] private float behindDistance = 1.5f;
        [SerializeField] private GameObject munchModel;

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
            if (cameraAnimation != null) {
                cameraAnimation.PlayMunchDeathAnimation();
                if (this.munchModel != null) {
                    this.munchModel.SetActive(false);
                }
            } else {
                Debug.LogWarning("CameraAnimationController not found in children of main camera.");
            }
        }

        protected override void OnTimerFinished()
        {
            KillPlayer();
        }
    }
}
