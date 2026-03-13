using UnityEngine;

namespace MonsterSystem
{
    /// Munch teleports behind the player, jumpscares, and kills them.
    public class MunchDragState : MonsterState
    {
        [Header("Teleport")]
        [SerializeField] private Transform player;
        [SerializeField] private float behindDistance = 1.5f;

        [Header("Kill")]
        [SerializeField] private float killDelay = 1f;

        private const string DragTimer = "drag";

        public override void OnStateEnter(MonsterController controller)
        {
            if (player != null)
            {
                controller.transform.position = player.position - player.forward * behindDistance;
                controller.transform.LookAt(player);
            }

            controller.ResetTimer(DragTimer);

            MonsterAudio.Stop(controller.Audio);

            var config = controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(controller.Audio, config.killJumpscareSound, 0.5f);
        }

        public override void OnStateTick(MonsterController controller, float tickDelta)
        {
            controller.TickTimer(DragTimer, tickDelta);

            if (controller.GetTimer(DragTimer) >= killDelay)
            {
                DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
            }
        }
    }
}
