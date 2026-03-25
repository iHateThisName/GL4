using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime collection of WindowControllers. Windows register/deregister themselves.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/Window Registry")]
public class SO_WindowRegistryRef : SO_RuntimeCollection<WindowController>
{
    private readonly List<WindowController> closedCache = new();

    public WindowController[] ClosedWindows
    {
        get
        {
            closedCache.Clear();
            var windows = Items;
            for (int i = 0; i < windows.Count; i++)
            {
                var w = windows[i];
                if (w != null && w.GetCurrentWindowState() == VRLever.EnumLeverState.Closed)
                    closedCache.Add(w);
            }
            return closedCache.Count > 0
                ? closedCache.ToArray()
                : System.Array.Empty<WindowController>();
        }
    }
}
