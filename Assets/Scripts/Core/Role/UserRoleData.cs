using Unity.Netcode;
using UnityEngine;

public class UserRoleData : MonoBehaviour
{
    public ulong playerId;
    public Role role;

    public UserRoleData(ulong playerId, Role role)
    {
        this.playerId = playerId;
        this.role = role;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref role);
    }
}
