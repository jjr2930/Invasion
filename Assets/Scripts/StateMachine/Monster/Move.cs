using System;
using UnityEngine;
using UnityEngine.AI;

namespace States.Monster
{
    public class Move : MonsterStateBase
    {
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField, Range(1f, 10f)] float pathfindingInterval = 5f;
        [SerializeField] float lastPathfindingTime;

        NetworkPlayerCharacter user;

        protected override void Init(Animator animator)
        {
            navMeshAgent = animator.GetComponent<NavMeshAgent>();
            base.Init(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (lastPathfindingTime + pathfindingInterval > Time.time)
                return;

            if (user == null)
            {
                user = FindClosestPlayer(animator.transform);
                if (user == null)
                {
                    //notfound...
                    return;
                }
            }


            if (!navMeshAgent.isOnNavMesh)
                return;

            Debug.Log($"this:{animator.gameObject.name} re pathfinding find : {user.name}");
            
            navMeshAgent.SetDestination(user.transform.position);
            lastPathfindingTime = Time.time;
        }

        private NetworkPlayerCharacter FindClosestPlayer(Transform thisTransform)
        {
            NetworkPlayerCharacter closestCharacter = null;
            float closestDistance = float.MaxValue;

            NetworkPlayerCharacter[] allCharacters = FindObjectsByType<NetworkPlayerCharacter>();

            foreach (var charcter in allCharacters)
            {
                float distance = Vector3.Distance(thisTransform.position, charcter.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCharacter = charcter;
                }
            }

            return closestCharacter;
        }
    }
}