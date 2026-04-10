using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// State that plays a kill animation and sound, then triggers the player death system.
    /// Intended as a terminal state — no automatic transition out.
    /// </summary>
    public class KillPlayerState : AnimatedState
    {
        [SerializeField] private bool useAnimation = true;
        
        /// <summary>
        /// Plays the kill animation and sound, then notifies the DeathSystem to kill the player.
        /// </summary>
        public override void OnStateEnter()
        {
            // Trigger the kill animation on the monster's Animator
            TriggerAffordances<AnimationAffordance>();
            
            // immediate change if not waiting for animation to finish
            if (!useAnimation)
            {
                // Play the one-shot kill sound effect
                TriggerAffordances<AudioAffordance>();

                // Kill the player
                KillPlayer();
            }
        }

        public override void OnAnimationComplete()
        {
            // Play the one-shot kill sound effect
            TriggerAffordances<AudioAffordance>();

            // Kill the player
            KillPlayer();
        }
    }
}
