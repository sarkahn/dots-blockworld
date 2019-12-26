using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct BlockDelta
{
    public short BlockType;
    public int3 WorldPos;
}
