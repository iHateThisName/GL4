namespace MonsterSystem
{
    /// <summary>
    /// Static data bus that carries the Radio monster's active broadcast values.
    /// The <c>RadioActiveState</c> writes here each frame while disrupting; the
    /// <see cref="RadioSensor"/> on other monsters reads these values to modulate
    /// their aggression or enter danger mode.
    /// </summary>
    public static class RadioBroadcast
    {
        /// <summary>Multiplier applied to monster aggression while the radio is disrupted. Default is 1 (no change).</summary>
        public static float AggressionMultiplier = 1f;

        /// <summary>True while the radio is on a non-safe channel and actively threatening the player.</summary>
        public static bool IsDangerMode = false;

        /// <summary>
        /// Resets both broadcast values to their neutral defaults.
        /// Call this when the Radio monster exits its active-disruption state.
        /// </summary>
        public static void Clear()
        {
            AggressionMultiplier = 1f;
            IsDangerMode = false;
        }
    }
}
