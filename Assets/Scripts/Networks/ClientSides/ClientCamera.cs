using UnityEngine;

[DefaultExecutionOrder(ExecutionOrders.ClientCamera)]
public class ClientCamera : MonoBehaviour
{
    [SerializeField] Transform networkCameraTransform;
    [SerializeField] IngameConfig config;
    [SerializeField] float lerpSpeed = 5f;
    [SerializeField] float rotationLerpSpeed = 5f;

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

        Vector3 nextPosition = Vector3.Lerp(this.transform.position, networkCameraTransform.position, lerpSpeed * Time.deltaTime);
        Quaternion nextRotation = Quaternion.Slerp(this.transform.rotation, networkCameraTransform.rotation, rotationLerpSpeed * Time.deltaTime);

        this.transform.SetPositionAndRotation(nextPosition, nextRotation);
    }

    private void OnNetworkCameraSpawned(NetworkCamera camera)
    {
        if (NetworkManagerExtensions.GetInstance().IsClient)
        {
            networkCameraTransform = camera.transform;
        }
    }
}