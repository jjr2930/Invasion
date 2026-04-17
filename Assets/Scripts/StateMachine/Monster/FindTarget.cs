using System;
using UnityEngine;

namespace States.Monster
{
    public class FindTarget : MonsterStateBase
    {
        [SerializeField] float searchingDelay = 1f;
        [SerializeField] float nextSearchTime = 0f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            nextSearchTime = 0f;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (Time.time >= nextSearchTime)
            {
                nextSearchTime = Time.time + searchingDelay;

                NetworkPlayerCharacter[] founds = FindObjectsByType<NetworkPlayerCharacter>();
                if(founds.Length == 0)
                {
                    return;
                }
                
                if (founds.Length > 0)
                {
                    string key = "Target";
                        
                    blackboard.SetOrAdd(key, founds[0].transform);
                    
                    animator.SetTrigger("Complete");
                }
            }
        }
    }
}