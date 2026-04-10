using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkMonster : NetworkBehaviour
{
    [Header("Editor time references")]
    [SerializeField] Animator fsmAnimator;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] GameObject visualPrefab;

    [Header("Runtime references")]
    [SerializeField] Transform visualInstance;

    [Header("Interpolation")]
    [SerializeField] PositionLerper positionLerper;
    [SerializeField] RotationLerper rotationLerper;

    public void SetDestination(Vector3 destination)
    {
        if (!IsServer)
            return;

        navMeshAgent.SetDestination(destination);
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();

        if (IsClient)
        {
            visualInstance = Instantiate(visualPrefab, transform.position, transform.rotation).transform;
        }
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            visualInstance.SetPositionAndRotation(
                positionLerper.Lerp(visualInstance.position, transform.position),
                rotationLerper.Lerp(visualInstance.rotation, transform.rotation)
            );
        }
    }
}
