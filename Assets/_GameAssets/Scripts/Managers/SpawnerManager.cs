using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class SpawnerManager : NetworkBehaviour
{
    public static SpawnerManager Instance { get; private set; }

    [Header("Player Prefab")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Transform Lists")]
    [SerializeField] private List<Transform> _spawnPointTransformList;
    [SerializeField] private List<Transform> _respawnPointTransformList;

    private List<int> _availableSpawnIndexList = new List<int>();
    private List<int> _availableRespawnIndexList = new List<int>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        for (int i = 0; i < _spawnPointTransformList.Count; i++)
        {
            _availableSpawnIndexList.Add(i);
        }

        for (int i = 0; i < _respawnPointTransformList.Count; i++)
        {
            _availableRespawnIndexList.Add(i);
        }

        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if(_availableSpawnIndexList.Count == 0)
        {
            Debug.Log("No available Spawn Points!");
            return;
        }
        int randomIndex = Random.Range(0, _spawnPointTransformList.Count);
        int spawnIndex = _availableSpawnIndexList[randomIndex];
        _availableSpawnIndexList.RemoveAt(spawnIndex);

        Transform spawnPointTransform = _spawnPointTransformList[spawnIndex];
        GameObject playerInstance = Instantiate(_playerPrefab, spawnPointTransform.position, spawnPointTransform.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public void RespawnPlayer(int respawnTimer, ulong clientId)
    {
        StartCoroutine(RespawnPlayerCoroutine(respawnTimer, clientId));
    }

    private IEnumerator RespawnPlayerCoroutine(int respawnTimer, ulong clientId)
    {
        yield return new WaitForSeconds(respawnTimer);

        if (GameManager.Instance.GetGameState() != GameState.Playing) { yield break; }

        if (_respawnPointTransformList.Count == 0)
        {
            Debug.Log("No available Respawn Points!");
            yield break;
        }

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            yield break;
        }

        if(_availableRespawnIndexList.Count == 0)
        {
            for (int i = 0; i < _respawnPointTransformList.Count; i++)
            {
                _availableRespawnIndexList.Add(i);
            }
        }

        int randomIndex = Random.Range(0, _availableRespawnIndexList.Count);
        int respawnIndex = _availableRespawnIndexList[randomIndex];
        _availableRespawnIndexList.RemoveAt(randomIndex);

        Transform respawnPointTransform = _respawnPointTransformList[respawnIndex];
        NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if(playerNetworkObject == null)
        {
            Debug.LogError("Player Network Object is Null");
            yield break;
        }

        if(playerNetworkObject.TryGetComponent<Rigidbody>(out var playerRigidbody))
        {
            playerRigidbody.isKinematic = true;
        }

        if (playerNetworkObject.TryGetComponent<NetworkTransform>(out var playerNetworkTransform))
        {
            playerNetworkTransform.Interpolate = false;
            playerNetworkObject.GetComponent<PlayerVehicleVisualController>().SetVehicleVisualActive(0.1f);
        }

        playerNetworkObject.transform.SetPositionAndRotation(respawnPointTransform.position, respawnPointTransform.rotation);

        yield return new WaitForSeconds(0.1f);

        playerRigidbody.isKinematic = false;
        playerNetworkTransform.Interpolate = true;

        if (playerNetworkObject.TryGetComponent<PlayerNetworkController>(out var playerNetworkController))
        {
            playerNetworkController.OnPlayerRespawned();
        }

    }
}
