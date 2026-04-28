using UnityEngine;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [SerializeField] private GameObject[] interactorRoots;

    private bool interactorsEnabled;

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
        {
            foreach (var root in interactorRoots)
                if (root != null) root.SetActive(false);

            SceneTransition.OnBeforeFadeIn += EnableInteractors;
            Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactors disabled, waiting for OnBeforeFadeIn.");
        }
        else
        {
            Debug.LogWarning($"[XRSceneSwitch] ({sceneName}) No interactorRoots assigned.");
        }
    }

    private void Start()
    {
        // Fallback: no transition in progress (e.g. first scene load) — enable immediately.
        if (!interactorsEnabled && !SceneTransition.IsTransitioning)
            EnableInteractors();
    }

    private void OnDestroy()
    {
        SceneTransition.OnBeforeFadeIn -= EnableInteractors;
    }

    private void EnableInteractors()
    {
        SceneTransition.OnBeforeFadeIn -= EnableInteractors;
        interactorsEnabled = true;

        if (interactorRoots == null) return;
        foreach (var root in interactorRoots)
            if (root != null) root.SetActive(true);

        Debug.Log($"[XRSceneSwitch] ({gameObject.scene.name}) Interactors enabled.");
    }
}
