using UnityEngine;

namespace MonsterSystem
{
    /// Munch is angry - scarier audio, still listens for food.
    public class MunchAngryState : MunchFeedableState
    {
        protected override void OnFeedableEnter(MonsterController controller)
        {
            var config = controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.Play(controller.Audio, config.angryWarningSound, loop: true);
        }

        protected override void OnFeedableExit(MonsterController controller)
            => MonsterAudio.Stop(controller.Audio);
    }
}
