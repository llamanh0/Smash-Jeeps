using UnityEngine;
using Unity.Netcode;

public class PlayerInteractionController : NetworkBehaviour
{
    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;

    private bool _isCrashed;
    private bool _isShieldActive;
    private bool _isSpikeActive;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) { return; }

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();

        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        _isCrashed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider other)
    {
        if (!IsOwner) { return; }
        if (_isCrashed) { return; }
        if (GameManager.Instance.GetGameState() != GameState.Playing) { return; }

        CheckCollectibleCollision(other);
        CheckDamageableCollision(other);
    }

    private void CheckCollectibleCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController);
        }
    }

    private void CheckDamageableCollision(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            if (_isShieldActive)
            {
                Debug.Log("Shield Active: Damage Blocked");
                return;
            }

            CrashTheVehicle(damageable);
        }
    }

    private void CrashTheVehicle(IDamageable damageable)
    {
        damageable.Damage(_playerVehicleController);
        SetKillerUIRpc(damageable.GetKillerClientId(),
            RpcTarget.Single(damageable.GetKillerClientId(), RpcTargetUse.Temp));

        SpawnerManager.Instance.RespawnPlayer(damageable.GetRespawnTimer(), OwnerClientId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetKillerUIRpc(ulong killerClientId, RpcParams rpcParams)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerClientId, out var killerClient))
        {
            KillScreenUI.Instance.SetSmashUI("Batu");
        }
    }

    public void OnPlayerRespawned()
    {
        enabled = true;
        _isCrashed = false;
    }

    public void SetShieldActive(bool active) => _isShieldActive = active;
    public void SetSpikeActive(bool active) => _isSpikeActive = active;
}
