using Unity.Netcode;
using UnityEngine;


public static class NetworkManagerExtensions
{
    static NetworkManager _instance;
    public static NetworkManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = Object.FindAnyObjectByType<NetworkManager>();
        }

        return _instance;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        _instance = null;
    }


    public static NetworkSpawnManager SpawnManager => GetInstance().SpawnManager;
}

