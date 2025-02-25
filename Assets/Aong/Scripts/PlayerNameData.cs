using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerNameData : INetworkSerializable, IEquatable<PlayerNameData>
{
    private FixedString64Bytes name;

    public PlayerNameData(string playerName)
    {
        name = new FixedString64Bytes(playerName);
    }

    public string GetName()
    {
        return name.ToString();
    }

    public bool Equals(PlayerNameData other)
    {
        return name == other.name;
    }

    public override bool Equals(object obj)
    {
        if (obj is PlayerNameData other)
        {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
    }
}