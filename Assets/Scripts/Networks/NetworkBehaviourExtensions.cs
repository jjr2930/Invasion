using Unity.Netcode;
using UnityEngine.Assertions;


public static class NetworkBehaviourExtensions
{
    public static bool IsOwnedByLocalPlayer(this NetworkBehaviour networkBehaviour)
    {
        Assert.IsNotNull(networkBehaviour, "NetworkBehaviour cannot be null.");
        Assert.IsNotNull(NetworkManagerExtensions.GetInstance(), "NetworkManager.Singleton cannot be null.");

        if (networkBehaviour.OwnerClientId == NetworkManagerExtensions.GetInstance().LocalClientId)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

