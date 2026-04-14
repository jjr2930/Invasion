using UnityEngine;
using Unity.Multiplayer.PlayMode;
using System.Linq;

public class ServerSwitch : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("ServerSwitch Start");
        
        
        var tags = CurrentPlayer.Tags;
        if(null == tags)
        {
            Debug.Log("No tags found for the current player.");
            return;
        }

        if(0 == tags.Count)
        {
            Debug.Log("Current player has no tags.");
            return;
        }

        Debug.Log("Step1 ");
        foreach(var tag in tags)
        {
            Debug.Log($"Player Tag: {tag}");
        }
        Debug.Log("Step2");

        if(!CurrentPlayer.Tags.Contains("Server"))
            return;

        Debug.Log("Tag is Server!, try to start server");

        NetworkManagerExtensions.GetInstance().StartServer();

        NetworkManagerExtensions.GetInstance().OnServerStarted += OnServerStarted;
        NetworkManagerExtensions.GetInstance().OnServerStopped += OnServerStopped;
        NetworkManagerExtensions.GetInstance().OnClientDisconnectCallback += OnClientDisconnected;
        
    }

    void OnDestroy()
    {
        if(!CurrentPlayer.Tags.Contains("Server"))
            return;

        NetworkManagerExtensions.GetInstance().OnServerStarted -= OnServerStarted;
        NetworkManagerExtensions.GetInstance().OnServerStopped -= OnServerStopped;
        NetworkManagerExtensions.GetInstance().OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected. Remaining clients: {NetworkManagerExtensions.GetInstance().ConnectedClients.Count}");
        if(NetworkManagerExtensions.GetInstance().ConnectedClients.Count == 0)
        {
            NetworkManagerExtensions.GetInstance().Shutdown();
            Debug.Log("All clients disconnected. Server shutting down.");
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started successfully.");
    }

    private void OnServerStopped(bool isShutdown)
    {
        if(isShutdown)
        {
            Debug.Log("Server stopped. Attempting to restart...");  
            NetworkManagerExtensions.GetInstance().StartServer();       
        }
    }
}
