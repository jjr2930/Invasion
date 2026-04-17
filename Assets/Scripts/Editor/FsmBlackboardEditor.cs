using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


[CustomEditor(typeof(FsmBlackboard))]
public class FsmBlackboardEditor : Editor
{
    FsmBlackboard Script
    {
        get => target as FsmBlackboard;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ReadOnlyDictionary<string, BlackboardVariable> variables = Script.Varibles;

        using(new EditorGUILayout.VerticalScope())
        {
            foreach (var variable in variables)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                
                }
            }
        }
    }
}
