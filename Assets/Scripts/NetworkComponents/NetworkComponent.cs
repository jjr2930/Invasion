using Unity.Netcode;
using UnityEngine;

namespace Jy.NetworkComponents
{
    public class NetworkComponent : NetworkBehaviour
    {
        protected NetworkServer networkServer;

        [SerializeField] NetworkObject networkObject;
        protected virtual void Reset()
        {
            networkObject = GetComponent<NetworkObject>();
        }

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();

            networkServer = FindAnyObjectByType<NetworkServer>();

            if(IsServer)
            {
                RegisterServerSideListeners();
                //networkServer.NetworkWorld.Add(OwnerClientId, this);
            }
            else if(IsClient)
            {
                RegisterClientSideListeners();
            }
        }
    
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if(IsServer)
            {
                UnregisterServerSideListeners();
                //networkServer.NetworkWorld.Add(OwnerClientId, this);
            }
            else if(IsClient)
            {
                UnregisterClientSideListeners();
            }
        }

        public virtual void RegisterServerSideListeners() { }
        public virtual void UnregisterServerSideListeners() { }
        public virtual void RegisterClientSideListeners() { }
        public virtual void UnregisterClientSideListeners() { }

        public virtual void UpdateNetwork() { }
    }
}
