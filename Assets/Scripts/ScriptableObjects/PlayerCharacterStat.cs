using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerCharacterStat", menuName = "ScriptableObjects/PlayerCharacterStat")]
public class PlayerCharacterStat : ScriptableObject
{
    public int maxHealth;
    public int damage;
}
