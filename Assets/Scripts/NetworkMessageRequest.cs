using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct MoveRequest : INetworkSerializable
{
    public float m_moveX;
    public float m_moveZ;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_moveX);
        serializer.SerializeValue(ref m_moveZ);
    }
}

public struct RotateRequest : INetworkSerializable
{
    public float m_rotate;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_rotate);
    }
}
