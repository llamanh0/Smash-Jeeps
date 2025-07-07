using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera _playerCamera;

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);
    }
}
