using UnityEngine;

namespace MonsterSystem
{
    /// Munch teleports behind the player, jumpscares, and kills them after the timer duration.
    /// Set the duration field on MonsterStateWithTimer to control the kill delay.
    public class MunchDragState : MonsterStateWithTimer
    {
        [Header("Teleport")]
        [SerializeField] private float behindDistance = 1.5f;

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            Transform player = this.controller.Config.PlayerTarget;
            if (player != null)
            {
                this.controller.transform.position = player.position - player.forward * this.behindDistance;
                this.controller.transform.LookAt(player);
            }

            MonsterAudio.Stop(this.controller.Audio);

            var config = this.controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(this.controller.Audio, config.killJumpscareSound, 0.5f);
        }

        protected override void OnTimerFinished()
        {
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}
