using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Affordance that sets an animation parameter.
    /// Uses Animator from MonsterController.
    /// For completion detection, use AnimationStateChange on the Animator state.
    /// </summary>
    public class AnimationAffordance : StateAffordance
    {
        public enum ParamType { Trigger, Bool }

        [SerializeField] private ParamType paramType = ParamType.Trigger;
        [SerializeField] private EnumAnimationStates AnimationState;
        
        [System.Obsolete("Only temp until animation controllers are fixed")]
        [SerializeField] private string paramName;
        [SerializeField] private bool boolValue = true;

        private Animator animator;

        public override void Initialize(MonsterController owningController)
        {
            this.animator = owningController.Animator;
        }

        public override void Trigger()
        {
            if (this.animator == null) return;
            
            if (this.AnimationState != EnumAnimationStates.None)
            {
                MonsterAnimation.SetTrigger(animator, AnimationTriggers.GetTriggerHash(this.AnimationState));
            }
            else if (!string.IsNullOrEmpty(this.paramName))
            {
                switch (this.paramType)
                {
                    case ParamType.Trigger:
                        this.animator.SetTrigger(this.paramName);
                        break;
                    case ParamType.Bool:
                        this.animator.SetBool(this.paramName, this.boolValue);
                        break;
                }
            }
        }

        public override void Stop()
        {
            if (this.animator == null) return;
            
            if (!string.IsNullOrEmpty(this.paramName))
            {
                // For bools, set to opposite value on stop
                if (this.paramType == ParamType.Bool)
                    this.animator.SetBool(this.paramName, !this.boolValue);
            }
        }
    }
}
