using UnityEngine;

[DefaultExecutionOrder(ExecutionOrders.ClientCamera)]
public class ClientCamera : MonoBehaviour
{
    [SerializeField] Transform networkCameraTransform;
    [SerializeField] IngameConfig config;

    public void Start()
    {
        ClientEventBus.Networks.OnNetworkCameraSpawned += OnNetworkCameraSpawned;
    }

    private void OnDestroy()
    {
        ClientEventBus.Networks.OnNetworkCameraSpawned -= OnNetworkCameraSpawned;
    }


    private void LateUpdate()
    {
        if (null == networkCameraTransform)
        {
            //not spawned yet
            return;
        }

        this.transform.SetPositionAndRotation(networkCameraTransform.position, networkCameraTransform.rotation);
    }

    private void OnNetworkCameraSpawned(NetworkCamera camera)
    {
        if (NetworkManagerExtensions.GetInstance().IsClient)
        {
            networkCameraTransform = camera.transform;
        }
    }
}