using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Ensures this script is always attached to a GameObject that has an XRSimpleInteractable component
[RequireComponent(typeof(XRSimpleInteractable))]
public class FireMatchBox : MonoBehaviour
{
    [SerializeField] private GameObject matchPrefab;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    // Keeps track of the match we just spawned so we don't accidentally spawn infinite matches
    private GameObject currentSpawnedMatch;
    private void Awake()
    {
        // Automatically grab the required component if we did not assign it in the inspector
        this.interactable = GetComponent<XRSimpleInteractable>();

        // Find the master Interaction Manager in the scene if we haven't linked it manually
        if (this.interactionManager == null)
            this.interactionManager = Object.FindAnyObjectByType<XRInteractionManager>();
    }

    private void OnEnable()
    {
        // Start listening for when the player grabs or selects the matchbox
        if (this.interactable != null)
        {
            this.interactable.selectEntered.AddListener(OnSpawnerSelected);
        }
    }

    private void OnDisable()
    {
        // Stop listening when the matchbox is disabled or destroyed to prevent memory leaks
        if (this.interactable != null)
        {
            this.interactable.selectEntered.RemoveListener(OnSpawnerSelected);
        }
    }


    public void OnSpawnerSelected(SelectEnterEventArgs args)
    {
        // If a match is already spawned and active, stop here and don't spawn another one
        if (this.currentSpawnedMatch != null)
        {
            return;
        }

        // Identify exactly which hand (controller) interacted with the matchbox
        IXRSelectInteractor hand = args.interactorObject;

        // Spawn a new match exactly where the player's hand is currently positioned and rotated
        this.currentSpawnedMatch = Instantiate(this.matchPrefab, hand.transform.position, hand.transform.rotation);

        // Grab the interaction component from our newly spawned match
        XRGrabInteractable newGrabable = this.currentSpawnedMatch.GetComponent<XRGrabInteractable>();

        if (newGrabable != null)
        {
            // Force the player's hand to let go of the matchbox...
            this.interactionManager.SelectExit(hand, args.interactableObject);
            // ...and instantly force that same hand to grab the new match instead
            this.interactionManager.SelectEnter(hand, newGrabable);
        }
    }
}