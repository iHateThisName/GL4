using UnityEngine;

namespace MonsterSystem
{
    /// Munch teleports behind the player, jumpscares, and kills them.
    public class MunchDragState : MonsterStateWithTimer
    {
        [Header("Teleport")]
        [SerializeField] private Transform player;
        [SerializeField] private float behindDistance = 1.5f;

        [Header("Kill")]
        [SerializeField] private float killDelay = 1f;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            if (player != null)
            {
                controller.transform.position = player.position - player.forward * behindDistance;
                controller.transform.LookAt(player);
            }
            
            MonsterAudio.Stop(controller.Audio);

            var config = controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(controller.Audio, config.killJumpscareSound, 0.5f);
        }

        protected override void OnTimerTick()
        {
            if (!this.GetTime().Equals(this.killDelay)) return;
            
            OnStateExit();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}
