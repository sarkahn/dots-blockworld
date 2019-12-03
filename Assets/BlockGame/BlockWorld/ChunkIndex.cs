using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ChunkIndex : IComponentData
{
    public int2 value;
    public static implicit operator int2(ChunkIndex t) => t.value;
    public static implicit operator ChunkIndex(int2 v) => new ChunkIndex { value = v };
}
