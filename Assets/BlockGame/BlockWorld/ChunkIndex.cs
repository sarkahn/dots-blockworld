using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ChunkIndex : ISharedComponentData, IEquatable<ChunkIndex>
{
    public int3 value;


    public static implicit operator int3(ChunkIndex t) => t.value;
    public static implicit operator ChunkIndex(int3 v) => new ChunkIndex { value = v };
    
    public bool Equals(ChunkIndex other) => value.Equals(other.value);
    public override int GetHashCode() => value.GetHashCode();
}
