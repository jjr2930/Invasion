using System.Collections.Generic;
using Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace States
{
    public class PlayerCharactersGeneration : ServerStateBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] NetworkObject playerCharacterPrefab;
        [SerializeField] NetworkObject networkCameraPrefab;

        [SerializeField] List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        [SerializeField] float waitingTime = 0;
        [SerializeField] float startTime = 0;

        List<SpawnPoint> shuffledSpawnPoints = new List<SpawnPoint>();
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Debug.Log("Entering PlayerCharactersGeneration state.");

            spawnPoints.Clear();
            spawnPoints.AddRange(FindObjectsByType<SpawnPoint>());

            shuffledSpawnPoints.Clear();
            shuffledSpawnPoints.AddRange(spawnPoints);
            shuffledSpawnPoints.Shuffle();

            foreach (var client in NetworkManagerExtensions.GetInstance().ConnectedClientsList)
            {
                SpawnPoint randomPoint = shuffledSpawnPoints.Dequeue();

                NetworkObject spawnedCharacterNetworkObject = playerCharacterPrefab
                    .InstantiateAndSpawn(NetworkManagerExtensions.GetInstance(), client.ClientId);

                Assert.IsNotNull(spawnedCharacterNetworkObject, $"Failed to spawn player character for client {client.ClientId}.");

                //NetworkPlayerCharacter spawnedNetworkPlayerCharacter
                //    = spawnedCharacterNetworkObject.GetComponent<NetworkPlayerCharacter>();

                //spawnedNetworkPlayerCharacter.SetInitialPositionRpc(randomPoint.transform.position, Quaternion.identity);

                NetworkCharacterTransformController transformController
                    = spawnedCharacterNetworkObject.GetComponent<NetworkCharacterTransformController>();

                transformController.SetInitPositionRpc(randomPoint.transform.position, Quaternion.identity);

                NetworkPlayer netowrkPlayer = client.PlayerObject.GetComponent<NetworkPlayer>();
                Assert.IsNotNull(netowrkPlayer, $"The player object for client {client.ClientId} does not have a NetworkPlayer component.");

                NetworkObject spawnedCameraNetworkObject
                    = networkCameraPrefab.InstantiateAndSpawn(NetworkManagerExtensions.GetInstance(), client.ClientId);

                NetworkCamera spawnedNetworkCamera = spawnedCameraNetworkObject.GetComponent<NetworkCamera>();
                Assert.IsNotNull(spawnedNetworkCamera, $"Failed to spawn network camera for client {client.ClientId}.");

                ServerEventBus.Spawning.onNetworkCameraSpawned?.Invoke(spawnedNetworkCamera);

                //netowrkPlayer.SpawnedCharacter = transformController.not;
                spawnedNetworkCamera.SetFollowTargetRpc(spawnedCharacterNetworkObject.NetworkObjectId);
            }

            startTime = Time.time;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (Time.time > startTime + waitingTime)
                return;

            animator.SetTrigger("Complete");
        }
    }
}