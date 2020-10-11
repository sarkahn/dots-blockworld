using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    public struct VoxelWorldReader
    {
        public bool TryGetBlock(
            VoxelWorldState world,
            int3 worldPos,
            out ushort block)
        {
            int3 chunkIndex = Grid3D.WorldToCell(worldPos);
            block = 0;
            if (!TryGetBlocksArrayFromIndex(world, chunkIndex, out var blocks))
                return false;

            int blockIndex = Grid3D.WorldToIndex(worldPos);
            block = blocks[blockIndex];
            return true;
        }

        public bool TryGetBlocksArrayFromIndex(
            VoxelWorldState world,
            int3 chunkIndex,
            out NativeArray<ushort> blocks
            )
        {
            blocks = default;
            if (!TryGetRegionFromIndex(world, chunkIndex.xz, out Entity region))
                return false;

            if (!TryGetVoxelChunkFromIndex(world, chunkIndex, out Entity chunkEntity))
                return false;

            var blocksBuffer = world.BlocksFromEntity[chunkEntity];

            blocks = blocksBuffer.Reinterpret<ushort>().AsNativeArray();

            return true;
        }

        public bool TryGetVoxelChunkFromIndex(
            VoxelWorldState world,
            int3 chunkIndex,
            out Entity chunkEntity)
        {
            return world.ChunkMap.TryGetValue(chunkIndex, out chunkEntity);
        }

        public bool TryGetRegionFromIndex(
            VoxelWorldState world,
            int2 regionIndex,
            out Entity region)
        {
            return world.RegionMap.TryGetValue(regionIndex, out region);
        }
    }
}