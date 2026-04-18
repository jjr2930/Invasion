using Invasion.Tables.Runtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Invasion
{
    public class MonsterAttackCollider : MonoBehaviour, IAttackable
    {
        [SerializeField] MonsterStatTable table = null;
        [SerializeField] int tableKey = 0;
        [SerializeField] MonsterStat monsterStat = null;

        public int GetDamage()
        {
            Assert.IsNotNull(monsterStat, "monsterStat is null. Did you forget to set the table key?");
            return monsterStat.damage;
        }

        public void SetTableKey(int key)
        {
            tableKey = key;
            monsterStat = table[tableKey];
        }
    }
}
