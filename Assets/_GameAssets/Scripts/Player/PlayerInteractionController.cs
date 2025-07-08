using UnityEngine;
using Unity.Netcode;

public class PlayerInteractionController : NetworkBehaviour
{
    private PlayerSkillController _playerSkillController;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;

        _playerSkillController = GetComponent<PlayerSkillController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;

        if(other.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController);
        }
    }
}
