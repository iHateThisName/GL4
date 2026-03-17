namespace MonsterSystem
{
    /// Static data bus for the Radio monster's broadcast values.
    /// RadioActiveState writes here; RadioSensor on other monsters reads.
    public static class RadioBroadcast
    {
        public static float AggressionMultiplier = 1f;
        public static bool IsDangerMode = false;

        public static void Clear()
        {
            AggressionMultiplier = 1f;
            IsDangerMode = false;
        }
    }
}
