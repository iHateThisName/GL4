namespace MonsterSystem
{
    /// <summary>
    /// State that plays a kill animation and sound, then triggers the player death system.
    /// Intended as a terminal state — no automatic transition out.
    /// </summary>
    public class KillPlayerState : MonsterState
    {
        /// <summary>
        /// Plays the kill animation and sound, then notifies the DeathSystem to kill the player.
        /// </summary>
        public override void OnStateEnter()
        {
            // Trigger the kill animation on the monster's Animator
            TriggerAffordances<AnimationAffordance>();

            // Play the one-shot kill sound effect
            TriggerAffordances<AudioAffordance>();
            
            // Notify the death system that the player was killed by a monster
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}
