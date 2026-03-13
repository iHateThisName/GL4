using UnityEngine;

namespace MonsterSystem
{
    /// Teleports Munch to a random shadow position from config, then auto-transitions.
    public class MunchRelocateState : MonsterState
    {
        [SerializeField] private MonsterState nextState;
        [SerializeField] private string retractTrigger = "Munch";

        private int lastIndex = -1;

        public override void OnStateEnter(MonsterController controller)
        {
            MonsterAudio.Stop(controller.Audio);
            MonsterAnimation.SetTrigger(controller.Animator, retractTrigger);

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

            RequestTransition(controller, nextState);
        }
    }
}
