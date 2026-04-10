using Unity.Netcode;
using UnityEngine;

public class SpawnController : NetworkBehaviour
{
    [SerializeField] private Unity.Netcode.NetworkObject playerPrefab; // Inspector에 할당

    // 서버가 특정 클라이언트에게 플레이어를 생성할 때
    public void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;
        if (playerPrefab == null) { Debug.LogError("playerPrefab 미할당"); return; }

        var go = Instantiate(playerPrefab);
        // 인스턴스화한 게임오브젝트의 이름을 클라이언트 ID를 포함해 변경
        go.gameObject.name = $"NetworkPlayer_{clientId}";
        go.SpawnAsPlayerObject(clientId, true);
    }

    // 클라이언트가 서버에 플레이어 생성 요청을 보낼 때 사용 (RequireOwnership=false)
    [ServerRpc]
    public void RequestSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        SpawnPlayerForClient(rpcParams.Receive.SenderClientId);
    }

    // 서버에서 클라이언트 접속 시 자동 스폰을 원하면(서버 전용 코드)
    private void Start()
    {
        NetworkManagerExtensions.GetInstance().OnClientConnectedCallback += SpawnPlayerForClient;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManagerExtensions.GetInstance() != null)
            NetworkManagerExtensions.GetInstance().OnClientConnectedCallback -= SpawnPlayerForClient;
    }
}