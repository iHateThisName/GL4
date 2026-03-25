using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds runtime-only references to cross-system objects (e.g., the Player).
/// Objects are registered on Awake/Start (e.g., by GameManager).
/// Automatically reset before each play session via SO_RuntimeScriptableObject.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/References")]
public class SO_RuntimeReferences : SO_RuntimeScriptableObject
{
    private static SO_RuntimeReferences instance;

    /// <summary>
    /// Static accessor. Set in OnReset (BeforeSceneLoad) and lazily found as fallback.
    /// </summary>
    public static SO_RuntimeReferences Instance
    {
        get
        {
            if (instance == null)
            {
                // Fallback: find it if OnReset hasn't run yet (e.g., AfterSceneLoad timing)
                var found = Resources.FindObjectsOfTypeAll<SO_RuntimeReferences>();
                if (found.Length > 0)
                    instance = found[0];
            }
            return instance;
        }
        private set => instance = value;
    }

    [System.NonSerialized] private Transform player;
    [System.NonSerialized] private Radio radio;
    [System.NonSerialized] private ScreenFade screenFade;
    [System.NonSerialized] private List<WindowController> windows = new();

    public Transform Player { get => this.player; set => this.player = value; }
    public Radio Radio { get => this.radio; set => this.radio = value; }
    public ScreenFade ScreenFade { get => this.screenFade; set => this.screenFade = value; }

    /// <summary>
    /// Gets the runtime reference to the windows.
    /// </summary>
    public List<WindowController> Windows => this.windows;

    /// <summary>
    /// Adds a runtime reference for a window.
    /// </summary>
    public void RegisterWindow(WindowController window) => this.windows.Add(window);

    /// <summary>
    /// Removes a runtime reference for a window.
    /// </summary>
    public void DeregisterWindow(WindowController window) => this.windows.Remove(window);

    /// <summary>
    /// Gets only the closed windows. Reuses a shared list to avoid allocation.
    /// </summary>
    private readonly List<WindowController> closedWindowsCache = new();
    public WindowController[] ClosedWindows
    {
        get
        {
            closedWindowsCache.Clear();
            for (int i = 0; i < this.windows.Count; i++)
            {
                var w = this.windows[i];
                if (w != null && w.GetCurrentWindowState() == VRLever.EnumLeverState.Closed)
                    closedWindowsCache.Add(w);
            }
            return closedWindowsCache.Count > 0
                ? closedWindowsCache.ToArray()
                : System.Array.Empty<WindowController>();
        }
    }

    /// <summary>
    /// Clears all runtime references so the SO starts clean each play session.
    /// </summary>
    protected override void OnReset()
    {
        Instance = this;
        this.player = null;
        this.radio = null;
        this.screenFade = null;
        if (this.windows == null)
            this.windows = new List<WindowController>();
        else
            this.windows.Clear();
    }
}
