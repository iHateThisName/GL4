using UnityEngine;

namespace MonsterSystem
{
    [CreateAssetMenu(menuName = "Monsters/Monster Config")]
    public class MonsterConfig : ScriptableObject
    {
        [Header("General")]
        public float baseTickRate = 0.2f;

        [Header("Night Scaling")]
        public NightOverride[] nightOverrides;

        [System.Serializable]
        public struct NightOverride
        {
            public int nightNumber;

            [Tooltip("1.0 = normal, 0.5 = half patience (harder)")]
            public float patienceMultiplier;

            [Tooltip("1.0 = normal, 2.0 = double aggression")]
            public float aggressionMultiplier;

            [Tooltip("1.0 = normal speed")]
            public float speedMultiplier;
        }

        /// Returns the override for the given night, or defaults (1,1,1) if none defined.
        public NightOverride GetOverrideForNight(int night)
        {
            if (nightOverrides != null)
            {
                for (int i = 0; i < nightOverrides.Length; i++)
                {
                    if (nightOverrides[i].nightNumber == night)
                        return nightOverrides[i];
                }
            }

            return new NightOverride
            {
                nightNumber = night,
                patienceMultiplier = 1f,
                aggressionMultiplier = 1f,
                speedMultiplier = 1f
            };
        }
    }
}
