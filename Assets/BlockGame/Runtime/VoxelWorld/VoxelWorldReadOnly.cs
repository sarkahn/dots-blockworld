using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.BlockGame
{
    /// <summary>
    /// Provides read-only random access to the voxel world. Can be used in parallel jobs.
    /// Should be obtained from <see cref="VoxelWorldSystem.GetVoxelWorldReadOnly"/>.
    /// </summary>
    public struct VoxelWorldReadOnly
    {
        VoxelWorldState _world;
        VoxelWorldReader _reader;

        public VoxelWorldReadOnly(VoxelWorldState world)
        {
            _world = world;
            _reader = new VoxelWorldReader();
        }

        public ushort GetBlock(int3 worldPos)
        {
            _reader.TryGetBlock(_world, worldPos, out var block);
            return block;
        }

        public ushort GetBlock(int x, int y, int z) => GetBlock(new int3(x, y, z));

        public bool TryGetRegionFromIndex(int2 regionIndex, out Entity region)
        {
            return _reader.TryGetRegionFromIndex(_world, regionIndex, out region);
        }

        public bool TryGetBlocksArrayFromIndex(
            int3 chunkIndex,
            out NativeArray<ushort> blocks)
        {
            return _reader.TryGetBlocksArrayFromIndex(_world, chunkIndex, out blocks);
        }

        public bool TryGetVoxelChunkFromIndex(int3 chunkIndex, out Entity chunkEntity)
        {
            return _reader.TryGetVoxelChunkFromIndex(_world, chunkIndex, out chunkEntity);
        }

        public AdjacentChunkBlocks GetAdjacentChunkBlocks(int3 pos)
        {
            return new AdjacentChunkBlocks(_world, pos);
        }
    }
}