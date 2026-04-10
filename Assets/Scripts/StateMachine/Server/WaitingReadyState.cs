using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace States
{

    [Serializable]
    public class WaitingReadyState : ServerStateBehaviour
    {

        [SerializeField] IngameConfig config;

        Dictionary<ulong, bool> playerReadyMap = new Dictionary<ulong, bool>(16);

        [SerializeField] int readyCount = 0;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (null == server)
            {
                server = animator.gameObject.GetComponent<NetworkServer>();
                Assert.IsNotNull(server, "NetworkServer 컴포넌트를 찾을 수 없습니다.");
            }

            playerReadyMap.Clear();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            var evt = server.DequeueEvent();
        }

        protected override void HandleEvent(IStateEventParameter serverEvent)
        {
            base.HandleEvent(serverEvent);
            if (serverEvent is PlayerReadyParameter readyParam)
            {
                playerReadyMap[readyParam.PlayerId] = true;
                readyCount = GetReadyCount();
                if (readyCount >= config.maxPlayerCount)
                {
                    animator.SetTrigger("Complete");
                }
            }
        }

        int GetReadyCount()
        {
            int count = 0;
            foreach (var kvp in playerReadyMap)
            {
                if (kvp.Value)
                {
                    count++;
                }
            }
            return count;
        }
    }

}