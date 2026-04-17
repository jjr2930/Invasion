using UnityEngine;

[CreateAssetMenu(fileName = "IngameConfig", menuName = "Configs/IngameConfig")]
public class IngameConfig : ScriptableObject
{
    [SerializeField, Range(1, 16)] public int maxPlayerCount;

    [Header("Player movement options")]
    [SerializeField] public float moveSpeed = 10f;

    [SerializeField] public float customRtt = 0.1f;

    [SerializeField] public float maintainDuration = 20f;
}
