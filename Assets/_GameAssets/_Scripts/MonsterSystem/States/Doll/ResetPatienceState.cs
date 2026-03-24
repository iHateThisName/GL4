using UnityEngine;
using System.Collections;

namespace MonsterSystem
{
	public class ResetPatienceState: MonsterStateWithTimer
	{
        [SerializeField] private float patienceValue;

        protected override void OnTimerFinished() 
        {
            var patience = this.controller.GetSensor<DollSensor>();
            if (patience != null)
            {
                patience.ReducePatience(this.patienceValue);
            }
        }
    }
}