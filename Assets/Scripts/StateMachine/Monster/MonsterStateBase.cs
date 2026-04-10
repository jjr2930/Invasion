using UnityEngine;

namespace States.Monster
{
    public class MonsterStateBase : StateMachineBehaviour
    {
        [SerializeField] bool initialized = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (!initialized)
            {
                Init(animator);
            }
        }

        protected virtual void Init(Animator animator)
        {
            initialized = true;
        }
    }
}