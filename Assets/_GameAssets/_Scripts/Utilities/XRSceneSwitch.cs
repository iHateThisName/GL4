using System.Threading;
using UnityEngine;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [SerializeField] private GameObject[] interactorRoots;

    private void Awake()
    {
        string sceneName = gameObject.scene.name;

        if (xrOriginRef != null)
        {
            Transform previous = xrOriginRef.Value;
            if (previous != null && previous != this.transform.root)
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) Disabling previous XR origin: '{previous.name}' (scene: {previous.gameObject.scene.name})");
                previous.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) No previous XR origin to disable (ref was {(previous == null ? "null" : "self")})");
            }

            xrOriginRef.Value = this.transform.root;
            Debug.Log($"[XRSceneSwitch] ({sceneName}) Registered XR origin: '{this.transform.root.name}'");
        }
        else
        {
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) xrOriginRef is not assigned!");
        }

        if (interactorRoots != null && interactorRoots.Length > 0)
        {
            Debug.Log($"[XRSceneSwitch] ({sceneName}) Starting interactor cycle ({interactorRoots.Length} roots)");
            _ = CycleInteractors(Application.exitCancellationToken, sceneName);
        }
        else
        {
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) No interactorRoots assigned, skipping cycle.");
        }
    }

    private async Awaitable CycleInteractors(CancellationToken ct, string sceneName)
    {
        foreach (var root in interactorRoots)
        {
            if (root != null)
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) Disabling interactor: '{root.name}'");
                root.SetActive(false);
            }
        }

        await Awaitable.NextFrameAsync(ct);

        if (this == null)
        {
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) Destroyed before interactors could be re-enabled.");
            return;
        }

        foreach (var root in interactorRoots)
        {
            if (root != null)
            {
                Debug.Log($"[XRSceneSwitch] ({sceneName}) Re-enabling interactor: '{root.name}'");
                root.SetActive(true);
            }
        }

        Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactor cycle complete.");
    }
}
