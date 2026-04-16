using System.Collections;
using System.Linq;
using Unity.Multiplayer.PlayMode;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class AutoServerOrClientRunner : MonoBehaviour
{
    [SerializeField] NetworkObject networkServerPrefab;

    public void Start()
    {
        Debug.Log("ServerSwitch Start");


        var tags = CurrentPlayer.Tags;
        if (null == tags)
        {
            Debug.Log("No tags found for the current player.");
            return;
        }

        if (0 == tags.Count)
        {
            Debug.Log("Current player has no tags.");
            return;
        }

        Debug.Log("Step1 ");
        if (tags.ToList().Count > 1)
        {
            Debug.LogError("Current player has more than one tag, which is unexpected. Please check the player configuration.");
            return;
        }

        if (CurrentPlayer.Tags.Contains("Server"))
        {
            NetworkManagerExtensions.GetInstance().OnServerStarted += OnServerStarted;
            NetworkManagerExtensions.GetInstance().StartServer();
            Debug.Log("Started as Server.");
        }
        else if (CurrentPlayer.Tags.Contains("Client"))
        {
            NetworkManagerExtensions.GetInstance().OnClientStarted += OnClientStarted;
            NetworkManagerExtensions.GetInstance().StartClient();
            Debug.Log("Started as Client.");
        }
    }

    private void OnServerStarted()
    {
        networkServerPrefab.InstantiateAndSpawn(NetworkManagerExtensions.GetInstance());
    }

    private void OnClientStarted()
    {
        StartCoroutine(SendReadyRpc());
    }

    IEnumerator SendReadyRpc()
    {
        while (true)
        {
            NetworkServer networkServer = FindAnyObjectByType<NetworkServer>();
            if (networkServer != null)
            {
                networkServer.ReadyRpc();
                break;
            }

            yield return null;
        }
    }
}
