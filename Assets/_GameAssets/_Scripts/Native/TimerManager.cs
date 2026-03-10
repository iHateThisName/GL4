using System.Collections.Generic;

/// <summary>
/// Centralized manager that updates all active timers each frame.
/// Integrated into Unity's PlayerLoopSystem via SystemsBootstrap for optimal performance.
/// </summary>
/// <remarks>
/// Using a single update loop for all timers avoids the overhead of multiple MonoBehaviour.Update() calls.
/// Timers register themselves when started and deregister when disposed.
/// </remarks>
public static class TimerManager
{
    private static readonly List<Timer> timers = new(); // Active timers to update each frame
    private static readonly List<Timer> sweep = new(); // Temporary copy to allow safe iteration during modifications

    /// <summary>
    /// Registers a timer to receive Update() calls each frame.
    /// Called automatically by Timer.Start().
    /// </summary>
    /// <param name="timer">The timer to register.</param>
    public static void RegisterTimer(Timer timer) => timers.Add(timer);

    /// <summary>
    /// Removes a timer from the update loop.
    /// Called automatically by Timer.Dispose().
    /// </summary>
    /// <param name="timer">The timer to deregister.</param>
    public static void DeregisterTimer(Timer timer) => timers.Remove(timer);

    /// <summary>
    /// Updates all registered timers. Called each frame by the PlayerLoopSystem.
    /// Uses a sweep list to safely handle timers that dispose themselves during update.
    /// </summary>
    public static void UpdateTimers()
    {
        if (timers.Count == 0) return;

        // Copy to sweep list to allow safe modification during iteration
        sweep.RefreshWith(timers);
        foreach (var timer in sweep)
        {
            timer.Update();
        }
    }

    /// <summary>
    /// Disposes all timers and clears the manager.
    /// Called when exiting play mode in the editor.
    /// </summary>
    public static void Clear()
    {
        sweep.RefreshWith(timers);
        foreach (var timer in sweep)
        {
            timer.Dispose();
        }
        timers.Clear();
        sweep.Clear();
    }
}

/// <summary>
/// Extension methods for List operations.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Clears the list and repopulates it with items from the source collection.
    /// Useful for creating a safe iteration copy without allocating a new list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to refresh.</param>
    /// <param name="items">The items to populate the list with.</param>
    public static void RefreshWith<T>(this List<T> list, IEnumerable<T> items)
    {
        list.Clear();
        list.AddRange(items);
    }
}
