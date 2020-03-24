using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [InternalBufferCapacity(0)]
    public struct ChunkMeshIndices : IBufferElementData
    {
        public int value;
        public static implicit operator int(ChunkMeshIndices c) => c.value;
        public static implicit operator ChunkMeshIndices(int v) => new ChunkMeshIndices { value = v };
    } 
}
