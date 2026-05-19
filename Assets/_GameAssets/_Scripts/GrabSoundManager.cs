using System;
using UnityEngine;

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

    private void OnEnable() {
        this.leftHandHandler.OnGrabItem += HandleLeftGrabSound;
        this.leftHandHandler.OnReleaseItem += HandleLeftReleaseSound;
        this.rightHandHandler.OnGrabItem += HandleRightGrabSound;
        this.rightHandHandler.OnReleaseItem += HandleRightReleaseSound;
    }

    private void OnDisable() {
        this.leftHandHandler.OnGrabItem -= HandleLeftGrabSound;
        this.leftHandHandler.OnReleaseItem -= HandleLeftReleaseSound;
        this.rightHandHandler.OnGrabItem -= HandleRightGrabSound;
        this.rightHandHandler.OnReleaseItem -= HandleRightReleaseSound;
    }

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

    public enum EnumGrabActions { Grab = 0, Place = 1, Drop = 2 }
}
