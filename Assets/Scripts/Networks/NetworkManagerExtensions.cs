using Unity.Netcode;
using UnityEngine;


/// <summary>
/// NetworkManager.Singleton이 잘 안되어서 이 함수를 썻음
/// 2026.04.11 이후로는 NetworkManager.ServerTime, 같은 방법으로 사용중, 문제없으면 이 클래스는 삭제할 예정
/// </summary>
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

