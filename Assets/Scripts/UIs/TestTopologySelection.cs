using Unity.Netcode;
using UnityEngine;

public class TestTopologySelection : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Unity.Netcode.NetworkObject serverPrefab;
    [SerializeField] int serverTickRate = 30;
    [SerializeField] int clientTickRate = 60;
    private void Reset()
    {
        networkManager = GetComponent<NetworkManager>();
    }
    private void Start()
    {
        networkManager.OnServerStarted += OnServerStarted;
        Application.targetFrameRate = 60;
    }

    public void OnClickedServerStart()
    {
        networkManager.StartServer();
        gameObject.SetActive(false);
    }

    public void OnClickedClientStart()
    {
        networkManager.StartClient();
        gameObject.SetActive(false);
    }

    private void OnServerStarted()
    {
        Debug.Log("ServerStarted");
        Unity.Netcode.NetworkObject spawnedServer = serverPrefab.InstantiateAndSpawn(networkManager);
    }
}
