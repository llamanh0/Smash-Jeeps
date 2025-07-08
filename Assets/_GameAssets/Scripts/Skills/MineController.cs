using UnityEngine;
using Unity.Netcode;

public class MineController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _mineCollider;

    [Header("Settings")]
    [SerializeField] private float _fallSpeed;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private LayerMask _groundLayer;

    private bool _hasLanded;
    private Vector3 _lastSentPosition;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            SetOwnerVisualsRpc();
        }
    }

    private void Update()
    {
        if(!IsServer || _hasLanded) return;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
        {
            _hasLanded = true;
            transform.position = hit.point;

            if(_lastSentPosition != transform.position)
            {
                SyncPositionRpc(transform.position);
                _lastSentPosition = transform.position;
            }
        }
        else
        {
            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SyncPositionRpc(Vector3 newPosition)
    {
        if(IsServer) { return; }

        transform.position = newPosition;
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _mineCollider.enabled = false;
    }
}
