using UnityEngine;

namespace States.Server
{
    public class MaintainState : ServerStateBehaviour
    {
        [SerializeField] IngameConfig ingameConfig;

        [SerializeField] float maintainEndTime = 0f;
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            base.OnStateMachineEnter(animator, stateMachinePathHash);

            maintainEndTime = Time.time + ingameConfig.maintainDuration;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (Time.time >= maintainEndTime)
            {
                animator.SetTrigger("Complete");
            }
        }
    }
}
