using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem {
    // Inga loves the Intruder and Munch :3 - They should kiss ^o^
    public class RelocateState : AnimatedState {
        [Header("=== Transitions ===")]
        //[SerializeField] private MonsterState nextState;
        [SerializeField] private float transitionDelay = 2.0f;

        [Header("=== VR & Physics Components ===")]
        [SerializeField] private XRGrabInteractable grabInteractable;

        [Header("=== Configuration ===")]
        [SerializeField] private SO_TransformCollection transforms;
        [SerializeField] private bool useConfig = true;
        [SerializeField] private bool canKillMomentum = true;

        [SerializeField] private Rigidbody rb;
        private int lastIndex = -1;
        private Coroutine delayRoutine;

        public override void Initialize(MonsterController owningController) {
            base.Initialize(owningController);
            this.rb = this.controller.transform.root.gameObject.GetComponent<Rigidbody>();
        }

        public override void OnStateEnter() {
            base.OnStateEnter();
            this.TriggerAffordances<VfxAffordance>();

            this.DisableGrabInteractable();
        }

        private void Teleport() {
            Vector3 targetPos = this.controller.transform.position;
            Quaternion targetRot = this.controller.transform.rotation;

            if (this.useConfig) {
                var spawnPoints = this.controller.SpawnPoints;
                if (spawnPoints != null && spawnPoints.points != null && spawnPoints.points.Length > 0) {
                    int index;
                    if (spawnPoints.points.Length == 1) {
                        index = 0;
                    } else {
                        do {
                            index = Random.Range(0, spawnPoints.points.Length);
                        }
                        while (index == this.lastIndex);
                    }

                    this.lastIndex = index;
                    targetPos = spawnPoints.points[index].position;
                    targetRot = Quaternion.Euler(spawnPoints.points[index].rotation);
                }
            } else {
                if (this.transforms != null && this.transforms.points.Length > 0) {
                    int index = Random.Range(0, this.transforms.points.Length);
                    targetPos = this.transforms.points[index].position;
                    targetRot = Quaternion.Euler(this.transforms.points[index].rotation);
                }
            }

            this.FixRigidBody(targetPos, targetRot); // for the doll
            this.controller.transform.root.SetPositionAndRotation(targetPos, targetRot);
            
            StartCoroutine(WaitAndTransitionRoutine());
        }

        public override void OnAnimationComplete() {
            base.OnAnimationComplete();
            Teleport();
        }

        private void FixRigidBody(Vector3 targetPos, Quaternion targetRot) {
            // THE PHYSICS FIX: Tell the Rigidbody exactly where it lives now.
            if (this.rb != null) {
                this.rb.position = targetPos;
                this.rb.rotation = targetRot;
            }
        }

        private void DisableGrabInteractable() {
            if (this.grabInteractable != null) {
                this.grabInteractable.enabled = false;
            }

            if (this.rb != null) {
                this.rb.isKinematic = true;

                if (this.canKillMomentum) {
                    this.rb.linearVelocity = Vector3.zero;
                    this.rb.angularVelocity = Vector3.zero;
                }
            }
        }

        private IEnumerator WaitAndTransitionRoutine() {
            yield return new WaitForSeconds(this.transitionDelay);

            if (this.nextState != null) {
                this.RequestTransition(this.nextState);
            }
        }
    }
}