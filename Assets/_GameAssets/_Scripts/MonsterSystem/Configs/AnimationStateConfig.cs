using UnityEngine;

namespace MonsterSystem
{
    [CreateAssetMenu(menuName = "Monsters/State Configs/Animation State Config")]
    public class AnimationStateConfig : ScriptableObject
    {
        public string enterTrigger;
        public string exitTrigger;
        public string loopParameter;
        public AudioClip loopAudio;
        [Range(0f, 1f)] public float audioVolume = 1f;
        public bool loopAudioEnabled = true;
    }
}
