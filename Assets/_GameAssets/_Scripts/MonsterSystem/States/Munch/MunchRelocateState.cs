using UnityEngine;

namespace MonsterSystem
{
    /// Teleports Munch to a random shadow position from config, then auto-transitions.
    public class MunchRelocateState : AnimatedState
    {
        private int lastIndex = -1;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            var config = controller.GetConfig<MunchConfig>();
            if (config != null && config.shadowPositions != null && config.shadowPositions.Length > 0)
            {
                int index;
                if (config.shadowPositions.Length == 1)
                {
                    index = 0;
                }
                else
                {
                    do
                    {
                        index = Random.Range(0, config.shadowPositions.Length);
                    }
                    while (index == lastIndex);
                }
                lastIndex = index;
                controller.transform.position = config.shadowPositions[index];
            }
            RequestTransition(nextState);
        }
    }
}
