using UnityEngine;

/// <summary>
/// Registers this XR origin in a shared SO_TransformRef and disables whatever origin
/// was previously registered before activating its own. This prevents two XR origins
/// from ever being active simultaneously without touching (and corrupting) this scene's
/// own XR origin configuration.
/// </summary>
public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;

    private void Awake()
    {
        if (xrOriginRef == null) return;

        // Disable the previous scene's XR origin before registering ours.
        // SceneTransition already disables it after fade-out, but this catches
        // any edge case (e.g. loading screen → new scene overlap).
        Transform previous = xrOriginRef.Value;
        if (previous != null && previous != this.transform.root)
            previous.gameObject.SetActive(false);

        xrOriginRef.Value = this.transform.root;
    }
}
