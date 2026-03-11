using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FireMatchBox : MonoBehaviour
{
    [SerializeField] private GameObject matchPrefab;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    private GameObject currentSpawnedMatch;

    private void Start()
    {
        this.interactable = GetComponent<XRSimpleInteractable>();
        if (this.interactionManager == null)
            this.interactionManager = Object.FindAnyObjectByType<XRInteractionManager>();
    }

    private void OnEnable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.AddListener(OnSpawnerSelected);
        }
    }

    private void OnDisable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.RemoveListener(OnSpawnerSelected);
        }
    }

    public void OnSpawnerSelected(SelectEnterEventArgs args)
    {
        if (this.currentSpawnedMatch != null)
        {
            return;
        }

        IXRSelectInteractor hand = args.interactorObject;

        this.currentSpawnedMatch = Instantiate(this.matchPrefab, hand.transform.position, hand.transform.rotation);

        XRGrabInteractable newGrabable = this.currentSpawnedMatch.GetComponent<XRGrabInteractable>();

        if (newGrabable != null)
        {
            this.interactionManager.SelectExit(hand, args.interactableObject);
            this.interactionManager.SelectEnter(hand, newGrabable);
        }
    }
}