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
        this.rightEmitter.Play();
        this.rightEmitter.SetParameter(PARAMETER_GRAB, (int)HandCollisionHandler.EnumGrabItems.Default);
        this.rightEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
        this.rightEmitter.Stop();

        this.leftEmitter.Play();
        this.leftEmitter.SetParameter(PARAMETER_GRAB, (int)HandCollisionHandler.EnumGrabItems.Default);
        this.leftEmitter.SetParameter(PARAMETER_ACTION, (int)EnumGrabActions.Grab);
        this.leftEmitter.Stop();
    }


    /// <summary>
    /// Updates the right emitter parameters and plays the grab sound when the right hand grabs an item.
    /// </summary>
    /// <param name="itemType">The type of item grabbed.</param>
    private void HandleRightGrabSound(HandCollisionHandler.EnumGrabItems itemType) {
        PlayGrabSound(itemType: itemType, action: EnumGrabActions.Grab, isRightHand: true);
    }
    /// <summary>
    /// Updates the right emitter parameters and plays the drop sound when the right hand releases an item.
    /// </summary>
    /// <param name="itemType">The type of item released.</param>
    private void HandleRightReleaseSound(HandCollisionHandler.EnumGrabItems itemType) {
        PlayGrabSound(itemType: itemType, action: EnumGrabActions.Drop, isRightHand: true);
    }

    /// <summary>
    /// Updates the left emitter parameters and plays the drop sound when the left hand releases an item.
    /// </summary>
    /// <param name="itemType">The type of item released.</param>
    private void HandleLeftReleaseSound(HandCollisionHandler.EnumGrabItems itemType) {
        PlayGrabSound(itemType: itemType, action: EnumGrabActions.Drop, isRightHand: false);
    }

    /// <summary>
    /// Updates the left emitter parameters and plays the grab sound when the left hand grabs an item.
    /// </summary>
    /// <param name="itemType">The type of item grabbed.</param>
    private void HandleLeftGrabSound(HandCollisionHandler.EnumGrabItems itemType) {
        PlayGrabSound(itemType: itemType, action: EnumGrabActions.Grab, isRightHand: false);
    }

    /// <summary>
    /// Defines the possible grab actions mapped to FMOD sound parameters.
    /// </summary>
    public enum EnumGrabActions { Grab = 0, Place = 1, Drop = 2 }

    [ContextMenu("Play Pick Up Default Sound")] public void PlayDefaultGrabSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Default, EnumGrabActions.Grab);
    [ContextMenu("Play Place Default Sound")] public void PlayDefaultPlaceSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Default, EnumGrabActions.Place);
    [ContextMenu("Play Drop Default Sound")] public void PlayDefaultDropSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Default, EnumGrabActions.Drop);
    [ContextMenu("Play Pick Up Axe Sound")] public void PlayAxeGrabSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Axe, EnumGrabActions.Grab);
    [ContextMenu("Play Place Axe Sound")] public void PlayAxePlaceSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Axe, EnumGrabActions.Place);
    [ContextMenu("Play Drop Axe Sound")] public void PlayAxeDropSound() => this.PlayGrabSound(HandCollisionHandler.EnumGrabItems.Axe, EnumGrabActions.Drop);

    private void PlayGrabSound(HandCollisionHandler.EnumGrabItems itemType, EnumGrabActions action, bool isRightHand = true) {
        if (isRightHand) {
            this.rightEmitter.Play();
            this.rightEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.rightEmitter.SetParameter(PARAMETER_ACTION, (int)action);
        } else {
            this.leftEmitter.Play();
            this.leftEmitter.SetParameter(PARAMETER_GRAB, (int)itemType);
            this.leftEmitter.SetParameter(PARAMETER_ACTION, (int)action);
        }
    }
}
