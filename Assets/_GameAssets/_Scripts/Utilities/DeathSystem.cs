using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static system that handles player death events and scene transitions.
/// Provides a centralized way to trigger death from any system (temperature, hunger, monster, etc.).
/// </summary>
/// <remarks>
/// Listeners can subscribe to OnPlayerDied to perform cleanup, show UI, or trigger effects before scene transition.
/// If no listeners are registered, the system automatically loads the GameOver scene.
/// </remarks>
public static class DeathSystem
{
    /// <summary>Fired when the player dies. Subscribe to handle death effects, UI, or transitions.</summary>
    public static event Action OnPlayerDied;

    /// <summary>Contains information about the current death event (reason, etc.).</summary>
    public static DeathEvent deathEvent;

    /// <summary>Returns true if any listeners are subscribed to OnPlayerDied.</summary>
    private static bool AnyListeners => OnPlayerDied?.GetInvocationList().Length > 0;

    /// <summary>
    /// Triggers player death with the specified reason.
    /// Notifies all subscribers via OnPlayerDied event.
    /// </summary>
    /// <remarks>
    /// If no listeners are registered, automatically loads the GameOver scene.
    /// Otherwise, listeners are responsible for calling deathEvent.LoadScene() when ready.
    /// </remarks>
    /// <param name="reason">The cause of death (Temperature, Hunger, Monster, or Survived).</param>
    /// <param name="completelyRestart">Reserved for future use - whether to fully restart the game.</param>
    public static void KillPlayer(DeathEvent.DeathReason reason, string additionalInfo = "", bool completelyRestart = false)
    {
        deathEvent = new DeathEvent(reason, additionalInfo);
        OnPlayerDied?.Invoke();

        // Automatically load scene if no listeners handle the transition
        if (!AnyListeners)
            deathEvent.LoadScene();
    }

    /// <summary>
    /// Resets the death system state.
    /// Called when exiting play mode in the editor.
    /// </summary>
    public static void Clear()
    {
        deathEvent = default;
    }

    /// <summary>
    /// Contains information about a death event including the reason and scene transition logic.
    /// </summary>
    [Serializable]
    public struct DeathEvent
    {
        /// <summary>
        /// The possible causes of player death or game end.
        /// </summary>
        public enum DeathReason
        {
            /// <summary>Player froze to death from hypothermia.</summary>
            Temperature,
            /// <summary>Player starved to death.</summary>
            Hunger,
            /// <summary>Player was killed by the monster.</summary>
            Monster,
            /// <summary>Player survived the night (win condition).</summary>
            Survived
        }

        private DeathReason reason;
        private string additionalInfo;

        /// <summary>
        /// Creates a new death event with the specified reason.
        /// </summary>
        /// <param name="reason">The cause of death.</param>
        public DeathEvent(DeathReason reason) : this(reason, "") {}
        
        public DeathEvent(DeathReason reason, string additionalInfo)
        {
            this.reason = reason;
            this.additionalInfo = additionalInfo;
        }

        /// <summary>
        /// Loads the GameOver scene.
        /// Called by listeners after handling death effects, or automatically if no listeners exist.
        /// </summary>
        public void LoadScene(SO_ScreenFadeRef fadeRef = null)
        {
            var config = reason == DeathReason.Survived
                ? FadeConfig.FadeToWhite()
                : FadeConfig.FadeToBlack();
            
            SceneTransition.LoadScene(2, config, fadeRef);
        }
        
        /// <summary>The cause of this death event.</summary>
        public DeathReason Reason => reason;
        
        /// <summary>Additional info, like the monster's name.</summary>
        public string AdditionalInfo => additionalInfo;
    }
}
