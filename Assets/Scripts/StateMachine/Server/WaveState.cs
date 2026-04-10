using MinMax;
using UnityEngine;
using UnityEngine.Assertions;

namespace States
{
    public class WaveState : ServerStateBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] MonsterSpawnPoint[] monsterSpawnPoints;
        [SerializeField] MinMaxFloat spawnDelayRange;
        [SerializeField] MinMaxInt spawnCountRange;
        [SerializeField] float nextSpawnTime;
        [SerializeField] NetworkMonsterSpawnHandler spawnHandler;

        [SerializeField] bool initialized = false;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (!initialized)
            {
                spawnHandler.Init();
                monsterSpawnPoints = FindObjectsByType<MonsterSpawnPoint>(FindObjectsInactive.Exclude);
                NetworkManagerExtensions.GetInstance().PrefabHandler.AddHandler(spawnHandler.PooledMonsterPrefab, spawnHandler);
                initialized = true;
            }
            Assert.IsNotNull(monsterSpawnPoints, "Monster prefab is not assigned.");
            Assert.IsTrue(monsterSpawnPoints.Length > 0, "No monster spawn points found in the scene.");
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (nextSpawnTime >= Time.time)
                return;

            nextSpawnTime = Time.time + spawnDelayRange.Random();

            int spawnCount = spawnCountRange.Random();

            for (int i = 0; i < spawnCount; i++)
            {

                int spawnPointIndex = Random.Range(0, monsterSpawnPoints.Length);
                MonsterSpawnPoint spawnPoint = monsterSpawnPoints[spawnPointIndex];
                NetworkManagerExtensions.SpawnManager.InstantiateAndSpawn(spawnHandler.PooledMonsterPrefab,
                    position: spawnPoint.transform.position,
                    rotation: spawnPoint.transform.rotation);
            }
        }
    }
}
