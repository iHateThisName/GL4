using UnityEngine;

namespace MonsterSystem
{
    public class KillPlayerState : MonsterState
    {
        [SerializeField] private string killAnimTrigger;
        [SerializeField] private AudioClip killSound;
        [SerializeField] [Range(0f, 1f)] private float killSoundVolume = 1f;

        public override void OnStateEnter()
        {
            MonsterAnimation.SetTrigger(controller.Animator, killAnimTrigger);
            MonsterAudio.PlayOneShot(controller.Audio, killSound, killSoundVolume);

            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}
