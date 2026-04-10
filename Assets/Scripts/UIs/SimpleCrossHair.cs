using UnityEngine;

public class SimpleCrossHair : MonoBehaviour
{
    private void OnGUI()
    {
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 size = new Vector2(20, 20);
        Rect rect = new Rect(center, size);
        Color originColor = GUI.color;
        GUI.color = Color.red;
        GUI.Label(rect, "*");
        GUI.color = originColor;
    }
}
