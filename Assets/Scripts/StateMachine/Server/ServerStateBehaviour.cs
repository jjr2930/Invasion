using UnityEngine;

namespace States
{
    public class ServerStateBehaviour : StateMachineBehaviour
    {
        [Header("Intialized in runtime")]
        [SerializeField] protected NetworkServer server;
        [SerializeField] protected Animator animator;

        [Header("Child properties")]
        [SerializeField, HideInInspector] protected int dummyInt;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            server = animator.GetComponent<NetworkServer>();
            this.animator = animator;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            IStateEventParameter evt = server.DequeueEvent();
            if (evt != null)
            {
                HandleEvent(evt);
            }
        }

        protected virtual void HandleEvent(IStateEventParameter serverEvent)
        {
            // 이벤트 처리 로직을 여기에 구현
        }
    }
}
