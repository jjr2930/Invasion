using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

public class BlackboardVariableGenericPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);

        Rect left = new Rect(position);
        Rect right = new Rect(position);

        left.width = left.width/2f;
        right.width = right.width/2f;

        left.center = left.center;
        right.center = left.center + new Vector2(left.width,0);

        var keyProperty = property.FindPropertyRelative("Key");
        var valueProperty = property.FindPropertyRelative("Value");

        EditorGUI.PropertyField(left, keyProperty);
        EditorGUI.PropertyField(right, valueProperty);
    }
}