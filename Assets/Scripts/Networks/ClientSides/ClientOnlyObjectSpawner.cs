using Unity.Cinemachine;
using UnityEngine;

public class ClientOnlyObjectSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] CinemachineBrain cameraPrefab;

    [Header("Spawned Objects")]
    [SerializeField] CinemachineBrain spawnedCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientEventBus.Networks.OnLocalPlayerCharacterSpawned += OnLocalPlayerCharacterSpawned;
    }

    private void OnDestroy()
    {
        ClientEventBus.Networks.OnLocalPlayerCharacterSpawned -= OnLocalPlayerCharacterSpawned;
    }

    private void OnLocalPlayerCharacterSpawned(NetworkPlayerCharacter character)
    {
        spawnedCamera = Instantiate(cameraPrefab);
    }
}
