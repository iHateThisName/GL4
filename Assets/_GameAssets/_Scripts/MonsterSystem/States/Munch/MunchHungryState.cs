using UnityEngine;

namespace MonsterSystem
{
    /// Munch is hungry - arm extends, hungry sound loops, listens for food.
    public class MunchHungryState : MunchFeedableState
    {
        [SerializeField] private string extendTrigger = "Return";

        protected override void OnFeedableEnter(MonsterController controller)
        {
            MonsterAnimation.SetTrigger(controller.Animator, extendTrigger);

            var config = controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.Play(controller.Audio, config.hungrySound, loop: true);
        }

        protected override void OnFeedableExit(MonsterController controller)
            => MonsterAudio.Stop(controller.Audio);
    }
}
