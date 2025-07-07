using UnityEngine;
using Unity.Netcode;

public class PlayerInteractionController : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;
        
        if(other.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect();
        }
    }
}
