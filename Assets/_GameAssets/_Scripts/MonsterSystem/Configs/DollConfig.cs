using UnityEngine;


namespace MonsterSystem
{
    [CreateAssetMenu(menuName = "Monsters/Doll Config")]
    public class DollConfig : ScriptableObject
    {
        [Header("Attention & Distance")]
        public float attentionRadius = 3.0f;
        public float attackDistance = 1.5f;

        [Header("Timers")]
        public float timeToHiding = 5.0f;
        public float timeToAggressive = 5.0f;
    }
}