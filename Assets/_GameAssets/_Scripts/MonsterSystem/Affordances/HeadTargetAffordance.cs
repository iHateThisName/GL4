using UnityEngine;

namespace MonsterSystem
{
    public class HeadTargetAffordance : StateAffordance
    {
        [SerializeField] private MunchFollow followTarget;
        [SerializeField] private Transform activeTarget;
        
        private Transform originalTarget;

        public override void OnTrigger()
        {
            this.originalTarget = this.followTarget.CurrentTarget;
            
            if (this.followTarget != null)
                this.followTarget.SetTarget(this.activeTarget);
        }
        
        public override void OnStop()
        {
            if (this.followTarget != null)
                this.followTarget.SetTarget(this.originalTarget);
        }
    }
}