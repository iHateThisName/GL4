using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRObjectDistanceBreak : XRGrabInteractable
{
    [SerializeField]
    [Tooltip("Maximum distance before forced release")]
    private float maxBreakDistance = 0.5f;

    [SerializeField] private bool useHapticWarning = true;
    [SerializeField] private float hapticIntensity = 0.5f;

    // Private Fields
    private float hapticWarningThreshold = 0.8f;

    // Unity Lifecycle Methods
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (this.isSelected)
        {
            this.CheckGrabDistance();
        }
    }

    // Getters / Setters
    public float GetMaxBreakDistance() => this.maxBreakDistance;
    public void SetMaxBreakDistance(float newDistance) => this.maxBreakDistance = newDistance;

    // Other Methods
    private void CheckGrabDistance()
    {
        // Calculate distance from the first interactor to the object's position
        Vector3 interactorPosition = this.interactorsSelecting[0].transform.position;
        float currentDistance = Vector3.Distance(interactorPosition, this.transform.position);

        if (currentDistance > this.maxBreakDistance)
        {
            this.BreakConnection();
        }
        else if (this.useHapticWarning && currentDistance > (this.maxBreakDistance * this.hapticWarningThreshold))
        {
            this.SendHapticFeedback();
        }
    }

    private void BreakConnection()
    {
        this.interactionManager.SelectExit(this.interactorsSelecting[0], this);
    }

    private void SendHapticFeedback()
    {
        if (this.interactorsSelecting[0] is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(this.hapticIntensity, 0.1f);
        }
    }
}