using UnityEngine;

public class TestServerTimeUI : MonoBehaviour
{
    public void OnGUI()
    {
        Rect rect = new Rect(Screen.width / 2, 0, 300, 200);
        GUI.Label(rect, $"ServerTime: {NetworkManagerExtensions.GetInstance().ServerTime.Time}");
    }
}
