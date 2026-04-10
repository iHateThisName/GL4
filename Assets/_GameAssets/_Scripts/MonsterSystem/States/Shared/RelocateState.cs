using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class RelocateState : MonsterState
    {
        [Header("=== Transitions ===")]
        [SerializeField] private MonsterState nextState;
        [SerializeField] private float transitionDelay = 2.0f;

        [Header("=== VR & Physics Components ===")]
        [Tooltip("Forces the player to drop her before teleporting.")]
        //[SerializeField] private XRGrabInteractable grabInteractable;

        // Notice we completely deleted the mainCollider variable!

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

            // 1. FORCE DROP: Drop from hands so the VR interaction system lets go.
            //if (this.grabInteractable != null) this.grabInteractable.enabled = false;

            // 2. LOCK PHYSICS: Keep her frozen so she doesn't fall through the floor 
            // or rubber-band while we wait for the 2-second timer.
            if (this.rb != null)
            {
                this.rb.isKinematic = true;

                if (this.killMomentum)
                {
                    this.rb.linearVelocity = Vector3.zero;
                    this.rb.angularVelocity = Vector3.zero;
                }
            }

            // 3. Handle Relocation (Leaving her collider ON so the bed realizes she left!)
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
                    var point = spawnPoints.points[index];

                    // USE RB.POSITION FOR RIGIDBODIES
                    this.rb.position = point.position;
                    this.rb.rotation = Quaternion.Euler(point.rotation);

                    // FORCE PHYSICS ENGINE TO UPDATE TRIGGERS IMMEDIATELY
                    //Physics.SyncTransforms();
                }
            }
            else
            {
                if (this.transforms != null && this.transforms.points.Length > 0)
                {
                    int index = Random.Range(0, this.transforms.points.Length);
                    Vector3 position = this.transforms.points[index].position;
                    Vector3 rotation = this.transforms.points[index].rotation;

                    // USE RB.POSITION FOR RIGIDBODIES
                    this.rb.position = position;
                    this.rb.rotation = Quaternion.Euler(rotation);

                    // FORCE PHYSICS ENGINE TO UPDATE TRIGGERS IMMEDIATELY
                    //Physics.SyncTransforms();
                }
            }

            // 4. Start the suspense timer
            delayRoutine = StartCoroutine(WaitAndTransitionRoutine());
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