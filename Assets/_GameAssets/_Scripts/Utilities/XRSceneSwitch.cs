using System.Threading;
using UnityEngine;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [Tooltip("Assign the Left and Right Controller GameObjects. Their children are cycled to reset interactor state.")]
    [SerializeField] private GameObject[] interactorRoots;

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
        {
            if (root == null) continue;
            foreach (Transform child in root.transform)
                child.gameObject.SetActive(false);
        }

        await Awaitable.NextFrameAsync(ct);

        if (this == null) return;

        foreach (var root in interactorRoots)
        {
            if (root == null) continue;
            foreach (Transform child in root.transform)
                child.gameObject.SetActive(true);
        }

        Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactor cycle complete.");
    }
}
