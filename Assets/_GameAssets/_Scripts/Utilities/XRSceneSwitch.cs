using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [SerializeField] private GameObject[] interactorRoots;
    [Tooltip("Assign the XRI Default Input Actions asset (or whichever asset drives your controllers). " +
             "All actions are reset after re-enable to prevent stale trigger state causing phantom selections.")]
    [SerializeField] private InputActionAsset actionAsset;

    private void Awake()
    {
        string sceneName = gameObject.scene.name;

        if (xrOriginRef != null)
        {
            Transform previous = xrOriginRef.Value;
            if (previous != null && previous != this.transform.root)
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) Disabling previous XR origin: '{previous.name}'");
                previous.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) No previous XR origin (ref was {(previous == null ? "null" : "self")})");
            }

            xrOriginRef.Value = this.transform.root;
            Debug.Log($"[XRSceneSwitch] ({sceneName}) Registered XR origin: '{this.transform.root.name}'");
        }
        else
        {
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) xrOriginRef is not assigned!");
        }

        if (interactorRoots != null && interactorRoots.Length > 0)
            _ = CycleInteractors(Application.exitCancellationToken, sceneName);
        else
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) No interactorRoots assigned.");
    }

    private async Awaitable CycleInteractors(CancellationToken ct, string sceneName)
    {
        foreach (var root in interactorRoots)
            if (root != null) root.SetActive(false);

        await Awaitable.NextFrameAsync(ct);

        if (this == null) return;

        foreach (var root in interactorRoots)
            if (root != null) root.SetActive(true);

        // Reset all input actions in the same frame as re-enable, before any Update() runs.
        // This forces every action's state machine back to waiting, so a continuously-held
        // trigger from the previous scene does not immediately register as a selection.
        // Only reset button/axis actions — skipping pose/tracking actions (Vector3, Quaternion)
        // so controller movement continues to work normally.
        if (actionAsset != null)
            foreach (var map in actionAsset.actionMaps)
                foreach (var action in map.actions)
                    if (action.expectedControlType is "Button" or "Axis")
                        action.Reset();

        Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactor cycle complete.");
    }
}
