using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace BlockGame.Chunks
{
    public struct VoxelChunkStack : IBufferElementData
    {
        public Entity Value;
        public static implicit operator Entity(VoxelChunkStack b) => b.Value;
        public static implicit operator VoxelChunkStack(Entity v) => new VoxelChunkStack { Value = v };
    }
}