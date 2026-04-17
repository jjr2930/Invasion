using System;
using Jy.NetworkComponents;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class NetworkComponentSwitcher : NetworkComponent
{
    [Serializable]
    public class SwitchInfo
    {
        public Behaviour component;
        public bool enableWhenServer;
        public bool enableWhenClient;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SwitchInfo))]
    public class SwitchInfoPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var componentProp = property.FindPropertyRelative("component");
            var enableWhenServerProp = property.FindPropertyRelative("enableWhenServer");
            var enableWhenClientProp = property.FindPropertyRelative("enableWhenClient");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            //가로로 3개 배치
            Rect componentRect = new Rect(position.x, position.y, position.width * 0.5f - spacing, lineHeight);
            Rect serverRect = new Rect(position.x + position.width * 0.5f,position.y, position.width * 0.25f - spacing, lineHeight);
            Rect clientRect = new Rect(position.x + position.width * 0.75f, position.y, position.width * 0.25f - spacing, lineHeight);

            EditorGUI.PropertyField(componentRect, componentProp, new GUIContent(""));
            EditorGUI.PropertyField(serverRect, enableWhenServerProp, new GUIContent(""));
            EditorGUI.PropertyField(clientRect, enableWhenClientProp, new GUIContent(""));
            EditorGUI.EndProperty();
        }
    }
    #endif

    [Header("Component | Enable when server | Enable when client")]
    [SerializeField] SwitchInfo[] switchInfos;
    

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsServer)
        {
            foreach (var switchInfo in switchInfos)
            {
                switchInfo.component.enabled = switchInfo.enableWhenServer;
            }
        }
        else if (IsClient)
        {
            foreach (var switchInfo in switchInfos)
            {
                switchInfo.component.enabled = switchInfo.enableWhenClient;
            }
        }
    }
}