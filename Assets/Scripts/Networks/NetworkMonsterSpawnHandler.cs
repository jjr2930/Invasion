using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class NetworkMonsterSpawnHandler : INetworkPrefabInstanceHandler
{
    [SerializeField] NetworkObject pooledMonsterPrefab;
    [SerializeField] int initCount = 100;
    [SerializeField] int limitCount = 100;

    ObjectPool<NetworkObject> pool = null;

    public NetworkObject PooledMonsterPrefab => pooledMonsterPrefab;

    public void Init()
    {
        if (pool != null)
            return;

        pool = new ObjectPool<NetworkObject>(
            createFunc: () =>
            {
                var obj = Object.Instantiate(pooledMonsterPrefab);
                obj.gameObject.SetActive(false);
                return obj;
            },
            actionOnGet: (obj) => { obj.gameObject.SetActive(true); },
            actionOnRelease: (obj) => { obj.gameObject.SetActive(false); },
            actionOnDestroy: (obj) =>
            {
                if (null == obj)
                    return;

                if (null == obj.gameObject)
                    return;

                Object.Destroy(obj.gameObject);
            },

            collectionCheck: true,
            initCount,
            limitCount
        );
    }

    public void Destroy(NetworkObject networkObject)
    {
        Debug.Log("Destroy called");
        pool.Release(networkObject);
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log("Instantiate called");
        NetworkObject obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }
}
