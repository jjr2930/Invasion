using UnityEngine;

[CreateAssetMenu(fileName = "IngameConfig", menuName = "Configs/IngameConfig")]
public class IngameConfig : ScriptableObject
{
    [SerializeField, Range(1, 16)] public int maxPlayerCount;


    [Header("Fixed Camera Options")]
    [SerializeField] public Vector2Int resolution = new Vector2Int(1920, 1080);
    [SerializeField] public float fov = 60f;
    [SerializeField] public float near = 0.1f;
    [SerializeField] public float far = 100f;
    [SerializeField] public float aspectRatio = 16f / 9f;

    [Header("Player movement options")]
    [SerializeField] public float moveSpeed = 10f;

    [SerializeField] public float customRtt = 0.1f;
}
