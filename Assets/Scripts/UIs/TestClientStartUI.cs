using UnityEngine;

public class TestClientStartUI : MonoBehaviour
{
    bool readyFlag = false;
    private void Update()
    {
        if (NetworkManagerExtensions.GetInstance() == null)
            return;

        if (!NetworkManagerExtensions.GetInstance().IsClient)
            return;

        if (!readyFlag)
        {
            NetworkServer server = FindAnyObjectByType<NetworkServer>();
            if (server != null && server.IsSpawned)
            {
                readyFlag = true;
                server.ReadyRpc();
            }
        }
    }
}
