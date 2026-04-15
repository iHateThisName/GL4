using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [SerializeField] private GameObject[] interactorRoots;
    [SerializeField] private InputActionReference[] interactionActions;
    [SerializeField] private XRBaseInputInteractor[] interactors;

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

        // Cancel selections through XRIT's proper API so OnSelectExited fires correctly
        // on both the interactor and the interactable.
        if (interactors != null)
            foreach (var interactor in interactors)
                if (interactor != null && interactor.interactionManager != null)
                    interactor.interactionManager.CancelInteractorSelection(interactor as IXRSelectInteractor);

        // Reset interaction actions to require a fresh press, preventing a held trigger
        // from the previous scene registering as a new selection.
        /*
        if (interactionActions != null)
            foreach (var actionRef in interactionActions)
                actionRef?.action?.Reset();*/

        Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactor cycle complete.");
    }
}
