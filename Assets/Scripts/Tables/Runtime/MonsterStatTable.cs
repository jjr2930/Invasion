using System;
using UnityEngine;

namespace Invasion.Tables.Runtime
{
    [Serializable] 
    public class MonsterStat : TableItem<int>
    {
        public int health;
        public int damage;
    }
    
    [CreateAssetMenu(fileName = "MonsterStatTable", menuName = "Tables/MonsterStatTable")]
    public class MonsterStatTable : TableBase<int, MonsterStat>
    {
    }
}
