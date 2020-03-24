using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [InternalBufferCapacity(0)]
    public struct ChunkMeshUVs : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(ChunkMeshUVs c) => c.value;
        public static implicit operator ChunkMeshUVs(float2 v) => new ChunkMeshUVs { value = v };
    } 
}
