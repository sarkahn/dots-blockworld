using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct BlockDelta : IBufferElementData
{
    public short BlockType;
    public int3 WorldPos;
}
