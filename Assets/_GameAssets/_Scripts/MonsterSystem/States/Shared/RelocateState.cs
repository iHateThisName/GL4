using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    // Inga loves the Intruder and Munch :3 - They should kiss ^o^
    public class RelocateState : MonsterState
    {
        [Header("=== Transitions ===")]
        [SerializeField] private MonsterState nextState;
        [SerializeField] private float transitionDelay = 2.0f;

        [Header("=== VR & Physics Components ===")]
        [SerializeField] private XRGrabInteractable grabInteractable;

        [Header("=== Configuration ===")]
        [SerializeField] private SO_TransformCollection transforms;
        [SerializeField] private bool useConfig = true;
        [SerializeField] private bool killMomentum = true;

        private Rigidbody rb;
        private int lastIndex = -1;
        private Coroutine delayRoutine;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.rb = this.controller.GetComponent<Rigidbody>();
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            TriggerAffordances<AudioAffordance>();
            
            DisableGrabInteractable();

            // 3. Handle Relocation (Using STRICT Rigidbody Math)
            Vector3 targetPos = this.controller.transform.position;
            Quaternion targetRot = this.controller.transform.rotation;

            if (this.useConfig)
            {
                var spawnPoints = this.controller.SpawnPoints;
                if (spawnPoints != null && spawnPoints.points != null && spawnPoints.points.Length > 0)
                {
                    int index;
                    if (spawnPoints.points.Length == 1) index = 0;
                    else
                    {
                        do { index = Random.Range(0, spawnPoints.points.Length); }
                        while (index == this.lastIndex);
                    }

                    this.lastIndex = index;
                    targetPos = spawnPoints.points[index].position;
                    targetRot = Quaternion.Euler(spawnPoints.points[index].rotation);
                }
            }
            else
            {
                if (this.transforms != null && this.transforms.points.Length > 0)
                {
                    int index = Random.Range(0, this.transforms.points.Length);
                    targetPos = this.transforms.points[index].position;
                    targetRot = Quaternion.Euler(this.transforms.points[index].rotation);
                }
            }

            FixRigidBody(targetPos, targetRot);
            this.controller.transform.root.SetPositionAndRotation(targetPos, targetRot);

            delayRoutine = StartCoroutine(WaitAndTransitionRoutine());
        }

        private void FixRigidBody(Vector3 targetPos, Quaternion targetRot)
        {
            // THE PHYSICS FIX: Tell the Rigidbody exactly where it lives now.
            if (this.rb != null)
            {
                this.rb.position = targetPos;
                this.rb.rotation = targetRot;
            }
        }

        private void DisableGrabInteractable()
        {
            if (this.grabInteractable != null) this.grabInteractable.enabled = false;

            if (this.rb != null)
            {
                this.rb.isKinematic = true;

                if (this.killMomentum)
                {
                    this.rb.linearVelocity = Vector3.zero;
                    this.rb.angularVelocity = Vector3.zero;
                }
            }
        }

        private IEnumerator WaitAndTransitionRoutine()
        {
            yield return new WaitForSeconds(transitionDelay);

            if (this.nextState != null)
            {
                RequestTransition(this.nextState);
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            if (delayRoutine != null)
            {
                StopCoroutine(delayRoutine);
                delayRoutine = null;
            }
        }
    }
}