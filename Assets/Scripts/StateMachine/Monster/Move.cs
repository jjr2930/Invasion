using System;
using MinMax;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace States.Monster
{
    public class Move : MonsterStateBase
    {
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] MinMaxFloat pathfindingIntervalRange ;
        [SerializeField] float nextPathfindingTime;
        [SerializeField] float stopDistance = 0.5f;
        [SerializeField] float refindingDistance = 3f;

        protected override void Init(Animator animator)
        {
            navMeshAgent = animator.GetComponent<NavMeshAgent>();
            base.Init(animator);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            Transform target = blackboard.Get<Transform>("Target"); 

            if (!navMeshAgent.isOnNavMesh)
                return;

            navMeshAgent.SetDestination(target.position);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (nextPathfindingTime > Time.time)
                return;

            nextPathfindingTime = Time.time + pathfindingIntervalRange.Random();

            Transform target = blackboard.Get<Transform>("Target"); 

            float agentDestinationSqrDistance = Vector3.SqrMagnitude(navMeshAgent.destination - target.position);
            bool targetPositionChanged = agentDestinationSqrDistance >= refindingDistance * refindingDistance;

            if(targetPositionChanged)
                navMeshAgent.SetDestination(target.position);

            bool isArrived = !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= stopDistance;
            if (isArrived)
                animator.SetTrigger("Complete");
        }
    }
}