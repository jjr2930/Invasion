using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace States.Monster
{
    public class AttackState: MonsterStateBase
    {
        [SerializeField] NetworkObject colliderPrefab;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            Assert.IsNotNull(colliderPrefab, $"colliderObject is null in {animator.gameObject.name}'s {GetType().Name}");
            
            Transform target = blackboard.Get<Transform>("Target");
            
            NetworkObject colliderInstance = Instantiate(colliderPrefab, animator.transform.position, Quaternion.identity);
            
            colliderInstance.Spawn();
        }
    }
}