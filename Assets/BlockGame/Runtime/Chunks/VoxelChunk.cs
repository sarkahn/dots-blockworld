
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    /// <summary>
    /// Represents a 16x16x16 section of blocks. Each value is the block type at a single position.
    /// </summary>
    [InternalBufferCapacity(Grid3D.CellVolume)]
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

        public int WorldHeight => Index.y * Grid3D.CellSizeY;
        public int3 WorldPosition => Index * Grid3D.CellSize;
    }

}
