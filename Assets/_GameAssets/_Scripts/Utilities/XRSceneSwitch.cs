using System.Threading;
using UnityEngine;

/// <summary>
/// Registers this XR origin in a shared SO_TransformRef and disables whatever origin
/// was previously registered before activating its own. This prevents two XR origins
/// from ever being active simultaneously without touching this scene's own XR origin.
///
/// Additionally cycles the assigned interactor roots for one frame so they reinitialize
/// from a clean state, clearing any stale hover/select carried over from controller input
/// during the loading screen.
/// </summary>
public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [Tooltip("Assign the Left and Right Controller GameObjects (children of the XR Origin). " +
             "They are disabled for one frame on load to clear stale interaction state.")]
    [SerializeField] private GameObject[] interactorRoots;

    private void Awake()
    {
        if (xrOriginRef != null)
        {
            // Disable the previous scene's XR origin before registering ours.
            Transform previous = xrOriginRef.Value;
            if (previous != null && previous != this.transform.root)
                previous.gameObject.SetActive(false);

            xrOriginRef.Value = this.transform.root;
        }

        if (interactorRoots != null && interactorRoots.Length > 0)
            _ = CycleInteractors(Application.exitCancellationToken);
    }

    // Disables all interactor roots for one frame then re-enables them.
    // This forces the XR interactors to re-evaluate input from a clean state,
    // preventing phantom hovers or selections carried in from controller button state.
    private async Awaitable CycleInteractors(CancellationToken ct)
    {
        foreach (var root in interactorRoots)
            if (root != null) root.SetActive(false);

        await Awaitable.NextFrameAsync(ct);

        if (this == null) return;

        foreach (var root in interactorRoots)
            if (root != null) root.SetActive(true);
    }
}
