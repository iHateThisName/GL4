using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Configuration ScriptableObject for the Stalker monster type.
    /// Extends <see cref="MonsterConfig"/> with stalker-specific audio settings.
    /// </summary>
    [CreateAssetMenu(menuName = "Monsters/Stalker Config")]
    public class StalkerConfig : MonsterConfig
    {
        [Header("Stalker - Audio")]
        public AudioClip stalkingAudio; // Audio clip played while the stalker is actively following the player
    }
}
