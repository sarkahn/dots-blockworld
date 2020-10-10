using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.Chunks
{
    /// <summary>
    /// Represents a 16x16x16 section of blocks. Each value is the block type at a single position.
    /// </summary>
    public struct VoxelChunkBlocks : IBufferElementData
    {
        public ushort BlockType;
        public static implicit operator ushort(VoxelChunkBlocks b) => b.BlockType;
        public static implicit operator VoxelChunkBlocks(ushort v) => new VoxelChunkBlocks { BlockType = v };
    } 

    public struct VoxelChunk : IComponentData
    {
        public int3 Index;
        public Entity Region;
    }
}
