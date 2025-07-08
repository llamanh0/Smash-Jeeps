using UnityEngine;
using Unity.Netcode;

public class RocketController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _rocketCollider;
    
    [Header("Settings")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;

    private bool _isMoving;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            SetOwnerVisualsRpc();
            RequestStartMovementFromServerRpc();
        }
    }

    private void Update() {
        if(IsServer && _isMoving)
        {
            MoveRocket();
        }
    }

    private void MoveRocket()
    {
        transform.position += transform.forward * _movementSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime, Space.Self);
    }

    [Rpc(SendTo.Server)]
    private void RequestStartMovementFromServerRpc()
    {
        _isMoving = true;
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _rocketCollider.enabled = false;
    }

    
}
