using UnityEngine;
using Unity.Netcode;

public struct SkillTransformDataSerializable : INetworkSerializeByMemcpy
{
    public Vector3 Position;
    public Quaternion Rotation;
    public SkillType SkillType;
    public NetworkObject NetworkObject;

    public SkillTransformDataSerializable(Vector3 position, Quaternion rotation, SkillType skillType, NetworkObject networkObject)
    {
        Position = position;
        Rotation = rotation;
        SkillType = skillType;
        NetworkObject = networkObject;
    }
}
