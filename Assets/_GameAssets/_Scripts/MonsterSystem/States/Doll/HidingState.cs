using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MonsterSystem
{
    public class HidingState : MonsterState
    {
        [Header("=== Components ===")]
        [SerializeField] private XRGrabInteractable grabInteractable;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private MonsterState nextState;

        private bool isTransitioning = false;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            this.isTransitioning = false;

            if (this.navMeshAgent != null)
            {
                this.navMeshAgent.enabled = false;
            }

            if (this.rb != null)
            {
                this.rb.isKinematic = false;
            }

            if (this.grabInteractable != null)
            {
                this.grabInteractable.enabled = true;
                // Listen for ANY time the doll is grabbed or snapped
                this.grabInteractable.selectEntered.AddListener(this.OnDollSelected);
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            if (this.grabInteractable != null)
            {
                this.grabInteractable.enabled = false;
                // Stop listening when she leaves this state
                this.grabInteractable.selectEntered.RemoveListener(this.OnDollSelected);
            }
        }

        private void OnDollSelected(SelectEnterEventArgs args)
        {
            // Check if the thing that just grabbed her is a Socket (not the player's hands)
            if (args.interactorObject is XRSocketInteractor)
            {
                Debug.Log("[HidingState] Snapped into a socket! Waiting for physical snap to finish...");

                if (this.isTransitioning)
                {
                    return;
                }

                if (this.nextState != null)
                {
                    this.isTransitioning = true;

                    // Call the new Coroutine
                    StartCoroutine(this.TransitionAfterSnapRoutine());
                }
                else
                {
                    Debug.LogError("[HidingState] Transition failed! 'Next State' is empty.");
                }
            }
        }

        private IEnumerator TransitionAfterSnapRoutine()
        {
            // Wait for the XR Toolkit to finish its visual/physical snapping movement.
            // Tweak this number slightly if it feels too fast or too slow.
            yield return new WaitForSeconds(0.25f);

            Debug.Log($"[HidingState] Snap finished, requesting transition to {this.nextState.name}");
            this.RequestTransition(this.nextState);
        }

        private IEnumerator TransitionNextFrameRoutine()
        {
            yield return new WaitForEndOfFrame();

            Debug.Log($"[HidingState] Frame ended, officially requesting transition to {this.nextState.name}");
            this.RequestTransition(this.nextState);
        }
    }
}