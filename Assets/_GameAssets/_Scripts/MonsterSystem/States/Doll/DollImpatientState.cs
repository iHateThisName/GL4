using UnityEngine;

namespace MonsterSystem
{
    public class DollImpatientState : MonsterState
    {
        public override void OnStateEnter()
        {
            Debug.Log("Doll is Impatient (Raising Head).");
            // Controller.Animator.SetTrigger("RaiseHead");
        }
    }
}