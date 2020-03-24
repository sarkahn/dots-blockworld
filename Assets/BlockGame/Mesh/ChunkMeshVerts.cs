using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [InternalBufferCapacity(0)]
    public struct ChunkMeshVerts : IBufferElementData
    {
        public float3 value;
        public static implicit operator float3(ChunkMeshVerts c) => c.value;
        public static implicit operator ChunkMeshVerts(float3 v) => new ChunkMeshVerts { value = v };
    } 
}
