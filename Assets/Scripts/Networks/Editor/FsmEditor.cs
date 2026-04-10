using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;



public class FsmEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Validate Animator Controller"))
        {
            var script = target as UnityEngine.Component;
            var runtimeController = script.GetComponent<Animator>().runtimeAnimatorController;

            if (runtimeController == null)
            {
                Debug.LogWarning("Animator Controller가 할당되어 있지 않습니다.");
                return;
            }

            var controller = runtimeController as AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning("AnimatorController로 캐스팅할 수 없습니다.");
                return;
            }

            int modifiedCount = 0;

            foreach (var layer in controller.layers)
            {
                ProcessStateMachine(layer.stateMachine, ref modifiedCount);
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log($"총 {modifiedCount}개의 트랜지션이 수정되었습니다. (hasExitTime=false, duration=0)");
        }
    }


    protected void ProcessStateMachine(AnimatorStateMachine stateMachine, ref int modifiedCount)
    {
        // AnyState 트랜지션 처리
        foreach (var transition in stateMachine.anyStateTransitions)
        {
            SetTransitionValues(transition, ref modifiedCount);
        }

        // 각 상태의 트랜지션 처리
        foreach (var state in stateMachine.states)
        {
            foreach (var transition in state.state.transitions)
            {
                SetTransitionValues(transition, ref modifiedCount);
            }
        }

        // 하위 상태 머신의 Entry/Exit 트랜지션 처리
        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            ProcessStateMachine(subStateMachine.stateMachine, ref modifiedCount);
        }
    }

    protected void SetTransitionValues(AnimatorStateTransition transition, ref int modifiedCount)
    {
        transition.hasExitTime = false;
        transition.duration = 0f;
        modifiedCount++;
    }
}
