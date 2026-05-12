using UnityEngine;
using UnityEngine.Events;

public class AnimationEventController : MonoBehaviour {

    private Animator animator;
    [SerializeField] private UnityEvent OnAnimationEvent; // Generic
    [SerializeField] private UnityEvent OnFootstepOutdoorEvent;
    [SerializeField] private UnityEvent OnFootstepIndoorEvent;

    void Start() {
        this.animator = GetComponent<Animator>();
    }

    public void HandleAnimationEvent() {
        this.OnAnimationEvent?.Invoke();
    }

    public void HandleFootstepOutdoorEvent() {
        this.OnFootstepOutdoorEvent?.Invoke();
    }

    public void HandleFootstepIndoorEvent() {
        this.OnFootstepIndoorEvent?.Invoke();
    }

}
