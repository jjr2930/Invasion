using UnityEngine;


public class PlayerCharacterStatGUI : MonoBehaviour
{
    public void OnGUI()
    {
        using (new GUILayout.VerticalScope("box"))
        {
            GUILayout.Label($"position : {transform.position}");
            GUILayout.Label($"Rotation : {transform.rotation.eulerAngles}");
        }
    }
}

