using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// State that plays a kill animation and sound, then triggers the player death system.
    /// Intended as a terminal state — no automatic transition out.
    /// </summary>
    public class KillPlayerState : MonsterState
    {
        [SerializeField] private string killAnimTrigger; // Animator trigger name for the kill animation
        [SerializeField] private AudioClip killSound; // Sound effect played when the kill occurs
        [SerializeField] [Range(0f, 1f)] private float killSoundVolume = 1f; // Volume scale for the kill sound

        /// <summary>
        /// Plays the kill animation and sound, then notifies the DeathSystem to kill the player.
        /// </summary>
        public override void OnStateEnter()
        {
            // Trigger the kill animation on the monster's Animator
            MonsterAnimation.SetTrigger(this.controller.Animator, this.killAnimTrigger);

            // Play the one-shot kill sound effect
            MonsterAudio.PlayOneShot(this.controller.Audio, this.killSound, this.killSoundVolume);

            // Notify the death system that the player was killed by a monster
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}
