using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FireMatchBox : MonoBehaviour
{
    //A refrence to the fire match prefab which will be spawned by the match box

    [SerializeField] private GameObject matchPrefab;
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private XRSimpleInteractable interactable;


    //A bool that is supposed to stop the match from duplicating, doesn't work
    private bool matchSpawned = false;
    private void Start()
    {
        // Cache the manager to handle the swap
        //interactionManager = GetComponentInParent<XRInteractionManager>();
        this.interactable = GetComponent<XRSimpleInteractable>();
        // If not in parent, try to find it in the scene
        if (this.interactionManager == null)
            this.interactionManager = Object.FindAnyObjectByType<XRInteractionManager>();
    }

    private void OnEnable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.AddListener(OnSpawnerSelected);
        }
        FireMatchController.OnMatchDespawn += FireMatchController_OnMatchDespawn;
    }
    private void OnDisable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.RemoveListener(OnSpawnerSelected);
        }
        FireMatchController.OnMatchDespawn -= FireMatchController_OnMatchDespawn;
    }

    private void FireMatchController_OnMatchDespawn()
    {
        this.interactable.enabled = true;
        this.matchSpawned = false;
    }


    public void OnSpawnerSelected(SelectEnterEventArgs args)
    {
        if (this.matchSpawned)
        {
            return;
        }
        // 1. Identify the Interactor (the hand)
        IXRSelectInteractor hand = args.interactorObject;

        // 2. Instantiate the new grabable object
        GameObject spawnedObj = Instantiate(this.matchPrefab/*, hand.transform.position, hand.transform.rotation*/);
        XRGrabInteractable newGrabable = spawnedObj.GetComponent<XRGrabInteractable>();

        if (newGrabable != null)
        {
            // 3. Force the hand to 'drop' the Spawner and 'grab' the new object
            // This happens in the same frame, so the player never sees the drop
            this.interactionManager.SelectExit(hand, args.interactableObject);
            this.interactionManager.SelectEnter(hand, newGrabable);
            this.matchSpawned=true;
        }
        this.interactable.enabled = false;
    }




}
