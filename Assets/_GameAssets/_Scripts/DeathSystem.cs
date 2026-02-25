using System;
using UnityEngine.SceneManagement;

public static class DeathSystem
{
    public static event Action OnPlayerDied;
    
    public static DeathEvent deathEvent;

    public static void KillPlayer(DeathEvent.DeathReason reason, bool completelyRestart = false)
    {
        deathEvent = new DeathEvent(reason);
        OnPlayerDied?.Invoke();
        SceneManager.LoadScene("GameOver");
    }

    public static void WinGame()
    {
        KillPlayer(DeathEvent.DeathReason.Survived, true);
    }

    public static void Clear()
    {
        deathEvent = default;
    }

    [Serializable]
    public struct DeathEvent
    {
        public enum DeathReason
        {
            Temperature,
            Hunger,
            Monster,
            Survived
        }

        private DeathReason reason;
        
        public DeathEvent(DeathReason reason)
        {
            this.reason = reason;
        }
        
        public DeathReason Reason => reason;
    }
}