using System;
using UnityEngine.SceneManagement;

public static class DeathSystem
{
    public static event Action OnPlayerDied;
    
    public static DeathEvent deathEvent;

    private static bool AnyListeners => OnPlayerDied?.GetInvocationList().Length > 0;

    public static void KillPlayer(DeathEvent.DeathReason reason, bool completelyRestart = false)
    {
        deathEvent = new DeathEvent(reason);
        OnPlayerDied?.Invoke();
        
        // Automatically restart the game if no listeners are registered, else let listeners handle it
        if (!AnyListeners)
            deathEvent.LoadScene();
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
        
        public void LoadScene()
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}