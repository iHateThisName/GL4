using UnityEngine;

namespace MonsterSystem
{
    public class DollPatientState : MonsterState
    {
        public override void OnStateEnter()
        {
            Debug.Log("Doll is Patient (Slouched).");
            // Controller.Animator.SetTrigger("Slouch");
        }
    }
}