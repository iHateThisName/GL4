using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

/// <summary>
/// Utility class for manipulating Unity's PlayerLoopSystem.
/// Provides methods to insert, remove, and debug custom update systems.
/// </summary>
/// <remarks>
/// Unity's PlayerLoopSystem controls the order and timing of all engine updates.
/// This utility allows injecting custom update callbacks (e.g., TimerManager)
/// at specific points in the loop without using MonoBehaviour.Update().
/// </remarks>
public static class PlayerLoopUtils
{
    /// <summary>
    /// Removes a system from the player loop hierarchy.
    /// Searches the current level first, then recursively checks all subsystems.
    /// </summary>
    /// <typeparam name="T">The parent system type to search within.</typeparam>
    /// <param name="loop">The player loop to modify (passed by reference).</param>
    /// <param name="systemToRemove">The system to find and remove.</param>
    public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
    {
        if (loop.subSystemList == null) return;

        // Search current level for matching system
        var playerLoopSystemList = new List<PlayerLoopSystem>(loop.subSystemList);
        for (int i = 0; i < playerLoopSystemList.Count; ++i)
        {
            // Match by both type and delegate to ensure correct system removal
            if (playerLoopSystemList[i].type == systemToRemove.type &&
                playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate)
            {
                playerLoopSystemList.RemoveAt(i);
                loop.subSystemList = playerLoopSystemList.ToArray();
                return;
            }
        }

        // Not found at this level, search deeper
        HandleSubSystemLoopForRemoval<T>(ref loop, systemToRemove);
    }

    /// <summary>
    /// Recursively searches subsystems for removal target.
    /// </summary>
    /// <typeparam name="T">The parent system type.</typeparam>
    /// <param name="loop">Current loop level to search.</param>
    /// <param name="systemToRemove">The system to remove.</param>
    static void HandleSubSystemLoopForRemoval<T>(ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove)
    {
        if (loop.subSystemList == null) return;

        for (int i = 0; i < loop.subSystemList.Length; ++i)
        {
            RemoveSystem<T>(ref loop.subSystemList[i], systemToRemove);
        }
    }

    /// <summary>
    /// Inserts a custom system into the player loop at a specific index.
    /// </summary>
    /// <typeparam name="T">The parent system type to insert under (e.g., Update, FixedUpdate).</typeparam>
    /// <param name="loop">The player loop to modify (passed by reference).</param>
    /// <param name="systemToInsert">The new system to add.</param>
    /// <param name="index">Position within the subsystem list (0 = first, before other systems).</param>
    /// <returns>True if insertion succeeded, false if parent type T was not found.</returns>
    public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
    {
        // Check if current loop matches target type
        if (loop.type != typeof(T)) return HandleSubSystemLoop<T>(ref loop, systemToInsert, index);

        // Found the target, insert the new system
        var playerLoopSystemList = new List<PlayerLoopSystem>();
        if (loop.subSystemList != null) playerLoopSystemList.AddRange(loop.subSystemList);
        playerLoopSystemList.Insert(index, systemToInsert);
        loop.subSystemList = playerLoopSystemList.ToArray();
        return true;
    }

    /// <summary>
    /// Recursively searches for the target system type T to insert into.
    /// </summary>
    /// <typeparam name="T">The parent system type to find.</typeparam>
    /// <param name="loop">Current loop level to search.</param>
    /// <param name="systemToInsert">The system to insert.</param>
    /// <param name="index">Target insertion index.</param>
    /// <returns>True if insertion succeeded in any subsystem.</returns>
    static bool HandleSubSystemLoop<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
    {
        if (loop.subSystemList == null) return false;

        for (int i = 0; i < loop.subSystemList.Length; ++i)
        {
            if (!InsertSystem<T>(ref loop.subSystemList[i], in systemToInsert, index)) continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Prints the entire player loop hierarchy to the Unity console.
    /// Useful for debugging to see all registered systems and their order.
    /// </summary>
    /// <param name="loop">The root player loop to print.</param>
    public static void PrintPlayerLoop(PlayerLoopSystem loop)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Unity Player Loop");
        foreach (PlayerLoopSystem subSystem in loop.subSystemList)
        {
            PrintSubsystem(subSystem, stringBuilder, 0);
        }
        Debug.Log(stringBuilder.ToString());
    }

    /// <summary>
    /// Recursively prints a subsystem and its children with indentation.
    /// </summary>
    /// <param name="system">The system to print.</param>
    /// <param name="stringBuilder">StringBuilder to append output.</param>
    /// <param name="level">Current indentation level (depth in hierarchy).</param>
    static void PrintSubsystem(PlayerLoopSystem system, StringBuilder stringBuilder, int level)
    {
        // Indent based on depth level
        stringBuilder.Append(' ', level * 2).AppendLine(system.type.ToString());
        if (system.subSystemList == null || system.subSystemList.Length == 0) return;

        // Recursively print children
        foreach (PlayerLoopSystem subSystem in system.subSystemList)
        {
            PrintSubsystem(subSystem, stringBuilder, level + 1);
        }
    }
}
