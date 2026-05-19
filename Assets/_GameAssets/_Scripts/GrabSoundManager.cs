using System;
using UnityEngine;

/// <summary>
/// Manages playing FMOD sounds when hands grab or release items, updating FMOD parameters based on the item type and action.
/// </summary>
public class GrabSoundManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] private HandCollisionHandler leftHandHandler;
    [SerializeField] private HandCollisionHandler rightHandHandler;

    [Header("Emitters")]
    [SerializeField] private FMODUnity.StudioEventEmitter leftEmitter;
    [SerializeField] private FMODUnity.StudioEventEmitter rightEmitter;

    private HandCollisionHandler.EnumGrabItems currentLeftParameter;
    private HandCollisionHandler.EnumGrabItems currentRightParameter;

    private EnumGrabActions currentLeftActionParameter;
    private EnumGrabActions currentRightActionParameter;

    private const string PARAMETER_GRAB = "Grabbing";
    private const string PARAMETER_ACTION = "Action";

    /// <summary>
    /// Subscribes to the grab and release events of both left and right hand handlers.
    /// </summary>
    private void OnEnable() {
        this.leftHandHandler.OnGrabItem += HandleLeftGrabSound;
        this.leftHandHandler.OnReleaseItem += HandleLeftReleaseSound;
        this.rightHandHandler.OnGrabItem += HandleRightGrabSound;
        this.rightHandHandler.OnReleaseItem += HandleRightReleaseSound;
    }

    /// <summary>
    /// Unsubscribes from the grab and release events of both left and right hand handlers.
    /// </summary>
    private void OnDisable() {
        this.leftHandHandler.OnGrabItem -= HandleLeftGrabSound;
        this.leftHandHandler.OnReleaseItem -= HandleLeftReleaseSound;
        this.rightHandHandler.OnGrabItem -= HandleRightGrabSound;
        this.rightHandHandler.OnReleaseItem -= HandleRightReleaseSound;
    }

    /// <summary>
    /// Initializes the default FMOD parameters for both left and right audio emitters.
    /// </summary>
    private void Start() {
        this.rightEmitter.SetParameter(PARAMETER_GRAB, (int)HandCollisionHandler.EnumGrabItems.Default);
        this.rightEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
        this.currentRightParameter = HandCollisionHandler.EnumGrabItems.Default;
        this.currentRightActionParameter = EnumGrabActions.Grab;

        this.leftEmitter.SetParameter(PARAMETER_GRAB, (int)HandCollisionHandler.EnumGrabItems.Default);
        this.leftEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
        this.currentLeftParameter = HandCollisionHandler.EnumGrabItems.Default;
        this.currentLeftActionParameter = EnumGrabActions.Grab;
    }


    /// <summary>
    /// Updates the right emitter parameters and plays the grab sound when the right hand grabs an item.
    /// </summary>
    /// <param name="itemType">The type of item grabbed.</param>
    private void HandleRightGrabSound(HandCollisionHandler.EnumGrabItems itemType) {
        if (this.currentRightParameter != itemType) {
            this.rightEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.currentRightParameter = itemType;
        }

        if (this.currentRightActionParameter != EnumGrabActions.Grab) {
            this.rightEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
            this.currentRightActionParameter = EnumGrabActions.Grab;
        }

        this.rightEmitter.Play();
    }
    /// <summary>
    /// Updates the right emitter parameters and plays the drop sound when the right hand releases an item.
    /// </summary>
    /// <param name="itemType">The type of item released.</param>
    private void HandleRightReleaseSound(HandCollisionHandler.EnumGrabItems itemType) {
        if (this.currentRightParameter != itemType) {
            this.rightEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.currentRightParameter = itemType;
        }

        if (this.currentRightActionParameter != EnumGrabActions.Drop) {
            this.rightEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Drop);
            this.currentRightActionParameter = EnumGrabActions.Drop;
        }

        this.rightEmitter.Play();
    }

    /// <summary>
    /// Updates the left emitter parameters and plays the drop sound when the left hand releases an item.
    /// </summary>
    /// <param name="itemType">The type of item released.</param>
    private void HandleLeftReleaseSound(HandCollisionHandler.EnumGrabItems itemType) {
        if (this.currentLeftParameter != itemType) {
            this.leftEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.currentLeftParameter = itemType;
        }

        if (this.currentLeftActionParameter != EnumGrabActions.Drop) {
            this.leftEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Drop);
            this.currentLeftActionParameter = EnumGrabActions.Drop;
        }

        this.leftEmitter.Play();

    }

    /// <summary>
    /// Updates the left emitter parameters and plays the grab sound when the left hand grabs an item.
    /// </summary>
    /// <param name="itemType">The type of item grabbed.</param>
    private void HandleLeftGrabSound(HandCollisionHandler.EnumGrabItems itemType) {
        if (this.currentLeftParameter != itemType) {
            this.leftEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.currentLeftParameter = itemType;
        }

        if (this.currentLeftActionParameter != EnumGrabActions.Grab) {
            this.leftEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
            this.currentLeftActionParameter = EnumGrabActions.Grab;
        }

        this.leftEmitter.Play();
    }

    /// <summary>
    /// Defines the possible grab actions mapped to FMOD sound parameters.
    /// </summary>
    public enum EnumGrabActions { Grab = 0, Place = 1, Drop = 2 }
}
