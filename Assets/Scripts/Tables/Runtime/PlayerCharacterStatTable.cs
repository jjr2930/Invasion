using System;
using UnityEngine;
using Invasion;
using Invasion.Commons.Runtime;

namespace Invasion.Tables.Runtime
{
    [Serializable]
    public class PlayerCharacterStat : TableItem<int>
    {
        public PlayerCharacterClass characterClass;
        public int health;
        public int dmg;
        public float moveSpeed;
    }

    [CreateAssetMenu(fileName = "PlayerCharacterStatTable", menuName = "Tables/PlayerCharacterStatTable")]
    public class PlayerCharacterStatTable 
        : TableBase<int, PlayerCharacterStat>
    {
    }
}
