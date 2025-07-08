using UnityEngine;
using Unity.Netcode;

public struct PositionDataSerializable : INetworkSerializeByMemcpy
{
    public Vector3 Position;

    public PositionDataSerializable(Vector3 position)
    {
        Position = position;
    }
}
