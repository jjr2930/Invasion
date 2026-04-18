using System;
using Invasion;
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

            NetworkObject spawnedCollider = colliderPrefab.InstantiateAndSpawn(NetworkManager.Singleton, 
                position: animator.transform.position,
                rotation: Quaternion.identity);
                
            MonsterAttackCollider attackCollider = spawnedCollider.GetComponent<MonsterAttackCollider>();
            attackCollider.SetTableKey(networkMonster.TableKey);
        }
    }
}