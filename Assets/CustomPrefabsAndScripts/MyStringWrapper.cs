using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public struct MyStringWrapper : INetworkSerializable
{
    public string Value;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // We'll just read/write the string in a raw manner:
        serializer.SerializeValue(ref Value);
    }
}
