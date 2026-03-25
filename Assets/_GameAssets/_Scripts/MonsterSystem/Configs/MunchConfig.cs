using UnityEngine;

namespace MonsterSystem
{
    [CreateAssetMenu(menuName = "Monsters/Munch Config")]
    public class MunchConfig : MonsterConfig
    {
        [Header("Munch - Satiety")]
        [Tooltip("Rate at which satiety drains per second (base rate).")]
        public float satietyDrainRate = 0.5f;

        [Tooltip("Reference: satiety threshold for hungry state.")]
        public float hungryThreshold = 40f;

        [Tooltip("Reference: satiety threshold for angry state.")]
        public float angryThreshold = 20f;

        [Tooltip("Satiety lost when an item is rejected.")]
        public float rejectPenalty = 5f;

        [Header("Munch - Audio")]
        public AudioClip hungrySound;
        public AudioClip angryWarningSound;
        public AudioClip killJumpscareSound;
        public AudioClip rejectSound;
        public AudioClip eatSound;
    }
}
