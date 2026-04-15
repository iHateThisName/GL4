using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRSceneSwitch : MonoBehaviour
{
    [SerializeField] private SO_TransformRef xrOriginRef;
    [SerializeField] private GameObject[] interactorRoots;
    [Tooltip("Assign the select/activate InputActionReferences directly from the left and right controllers.")]
    [SerializeField] private InputActionReference[] interactionActions;

    [SerializeField] private XRBaseInputInteractor[] kskgns;

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
        
        foreach (var interactor in kskgns)
            interactor.interactablesSelected.Clear();

        // Reset only the specific interaction actions in the same frame as re-enable,
        // before Update() processes them. Forces a fresh press requirement so a held
        // trigger from the previous scene cannot cause a phantom selection.
        if (interactionActions != null)
            foreach (var actionRef in interactionActions)
                actionRef?.action?.Reset();

        Debug.Log($"[XRSceneSwitch] ({sceneName}) Interactor cycle complete.");
    }
}
