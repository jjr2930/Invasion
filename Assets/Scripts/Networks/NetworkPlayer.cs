using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] NetworkPlayerCharacter spawnedCharacter;
    public NetworkPlayerCharacter SpawnedCharacter
    {
        get => spawnedCharacter;
        set => spawnedCharacter = value;
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
    }
}
