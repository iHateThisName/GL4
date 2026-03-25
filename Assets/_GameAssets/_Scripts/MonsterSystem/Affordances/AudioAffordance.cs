using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Affordance that plays audio when triggered.
    /// Uses AudioSource from MonsterController.
    /// Supports one-shot and looping audio.
    /// </summary>
    public class AudioAffordance : StateAffordance
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        
        private AudioSource audioSource;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.audioSource = this.controller?.Audio;
        }

        public override void Trigger()
        {
            if (this.audioSource == null || this.clip == null) return;

            if (this.loop)
                MonsterAudio.Play(audioSource, this.clip, true, this.volume);
            else
                MonsterAudio.PlayOneShot(this.audioSource, this.clip, this.volume);
        }

        public override void Stop()
        {
            if (this.audioSource != null)
                MonsterAudio.Stop(this.audioSource);
        }
    }
}
