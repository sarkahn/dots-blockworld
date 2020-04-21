using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public struct LoadChunksCursor
{
    public float3 WorldPos;
    public Color Color;
    public int Size { get; private set; }
    public int3 IndexPos => (int3)math.floor(WorldPos / Size);
    public float3 SnappedWorldPosition => IndexPos * Size;
    public Vector3 SnappedWorldCenter => SnappedWorldPosition + (Size / 2f);
    public Vector3 WorldSize => new float3(Size);


    public void MoveByIndex(int3 move)
    {
        WorldPos += move * Size;
    }

    public void Draw()
    {
        Handles.color = Color;
        Handles.DrawWireCube(SnappedWorldCenter, WorldSize);
    }

    public LoadChunksCursor(int size, Color color)
    {
        Size = size;
        WorldPos = 0;
        this.Color = color;
    }

    public LoadChunksCursor(LoadChunksCursor other)
    {
        WorldPos = other.WorldPos;
        Size = other.Size;
        Color = other.Color;
    }

}
