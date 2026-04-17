using MinMax;
using UnityEngine;
using UnityEngine.AI;

namespace States.Monster
{
    public class TargetLostChecker : MonsterStateBase
    {
        [SerializeField] MinMaxFloat  lostCheckIntervalRange;
        [SerializeField]float refindingDistance = 3f;
        float nextCheckTime = 0f;
        Transform target = null;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            target = blackboard.Get<Transform>("Target");
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + lostCheckIntervalRange.Random();

                if (target == null)
                {
                    Debug.Log("Target lost, target is null");
                    animator.SetTrigger("TargetLost");
                }

                float sqrDistance = Vector3.SqrMagnitude(animator.transform.position - target.position);
                float sqrRefindingDistance = refindingDistance * refindingDistance;
                if (sqrDistance >= sqrRefindingDistance)
                {
                    Debug.Log("Target lost, distance is too far");
                    animator.SetTrigger("TargetLost");
                }
            }
        }
    }
}
