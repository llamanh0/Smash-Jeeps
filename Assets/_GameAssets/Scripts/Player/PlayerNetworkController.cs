using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera _playerCamera;

    private PlayerVehicleController _playerVehicleController;
    private PlayerSkillController _playerSkillController;
    private PlayerInteractionController _playerInteractionController;

    public override void OnNetworkSpawn()
    {
        _playerCamera.gameObject.SetActive(IsOwner);

        if(!IsOwner) { return; }

        _playerVehicleController = GetComponent<PlayerVehicleController>();
        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerInteractionController = GetComponent<PlayerInteractionController>();
    }

    public void OnPlayerRespawned()
    {
        _playerVehicleController.OnPlayerRespawned();
        _playerSkillController.OnPlayerRespawned();
        _playerInteractionController.OnPlayerRespawned();
    }
}
