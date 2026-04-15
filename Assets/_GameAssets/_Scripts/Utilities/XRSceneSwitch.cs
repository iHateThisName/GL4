using UnityEngine;

/// <summary>
/// Registers this XR origin in a shared SO_TransformRef so SceneTransition can disable
/// it before the new scene activates. If the scene is loaded mid-transition the origin
/// is immediately disabled and re-enabled once the transition (and fade-in) is complete.
///
/// Set disableDuringTransition = false on the loading screen's XROrigin — it must stay
/// active so the player can see the loading UI in VR.
/// </summary>
public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [Tooltip("Disable this XR origin when loaded mid-transition. Uncheck for the loading screen scene.")]
    [SerializeField] private bool disableDuringTransition = true;

    private void Awake()
    {
        if (xrOriginRef != null)
            xrOriginRef.Value = this.transform.root;

        if (disableDuringTransition && SceneTransition.IsTransitioning)
            _ = DisableUntilTransitionDone(Application.exitCancellationToken);
    }

    private async Awaitable DisableUntilTransitionDone(System.Threading.CancellationToken ct)
    {
        this.transform.root.gameObject.SetActive(false);

        while (SceneTransition.IsTransitioning)
            await Awaitable.NextFrameAsync(ct);

        // Guard against the unlikely case this object was destroyed before the transition ended.
        if (this != null)
            this.transform.root.gameObject.SetActive(true);
    }
}
