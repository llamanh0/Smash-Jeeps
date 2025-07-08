using UnityEngine;
using Unity.Netcode;

public class SpikeController : NetworkBehaviour
{
    [SerializeField] private Collider _spikeCollider;

    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFinished += PlayerSkillController_OnTimerFinished;

        if(IsOwner)
        {
            SetOwnerVisualsRpc(OwnerClientId);
        }
    }

    private void PlayerSkillController_OnTimerFinished()
    {
        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if(IsServer)
        {
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc(ulong ownerClientId)
    {
        _spikeCollider.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= PlayerSkillController_OnTimerFinished;
    }

    
}
