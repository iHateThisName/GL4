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
            TriggerAffordances<VfxAffordance>();
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

        [Header("=== Easter Egg ===")]
        [SerializeField] private MonsterState attackState; // Assign the Attack State in the inspector

        private void OnDollSelected(SelectEnterEventArgs args)
        {
            // Check if the thing that just grabbed her is a Socket (not the player's hands)
            if (args.interactorObject is XRSocketInteractor socket)
            {
                // EASTER EGG: Check if the socket is the Fireplace
                if (socket.transform.CompareTag("FirePlace"))
                {
                    Debug.Log("[HidingState] Snapped into the Fireplace! Triggering Easter Egg.");

                    if (this.attackState != null)
                    {
                        // Instantly transition to the attack state
                        this.RequestTransition(this.attackState);
                    }
                    else
                    {
                        Debug.LogError("[HidingState] Fireplace triggered, but Attack State is missing!");
                    }

                    return; // Stop the rest of the normal bed-socket logic
                }

                Debug.Log("[HidingState] Snapped into a normal socket! Waiting for physical snap to finish...");

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