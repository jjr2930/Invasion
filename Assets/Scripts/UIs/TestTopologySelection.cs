using System;
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
        networkManager.OnServerStopped += OnServerStopped;
        networkManager.OnClientStarted += OnClientStarted;
        networkManager.OnClientStopped += OnClientStopped;

        Application.targetFrameRate = 60;
    }

    void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerStarted;
        networkManager.OnServerStopped -= OnServerStopped;
        networkManager.OnClientStarted -= OnClientStarted;
        networkManager.OnClientStopped -= OnClientStopped;
    }

    private void OnServerStopped(bool obj)
    {
        gameObject.SetActive(true);
    }

    private void OnClientStarted()
    {
        gameObject.SetActive(false);
    }

    private void OnClientStopped(bool obj)
    {
        gameObject.SetActive(true);
    }

    public void OnClickedServerStart()
    {
        networkManager.StartServer();
        Debug.Log("Server Started");
    }

    public void OnClickedClientStart()
    {
        networkManager.StartClient();
    }

    private void OnServerStarted()
    {
        Debug.Log("ServerStarted");
        Unity.Netcode.NetworkObject spawnedServer = serverPrefab.InstantiateAndSpawn(networkManager);

        gameObject.SetActive(false);
    }
}
