using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    public struct VoxelWorld
    {
        VoxelWorldReader _reader;
        VoxelWorldWriter _writer;
        VoxelWorldState _world;
        EntityCommandBuffer ECB => _writer.ECB;

        public VoxelWorld(VoxelWorldWriter writer, VoxelWorldState world)
        {
            _reader = new VoxelWorldReader();
            _writer = writer;
            _world = world;
        }

        public void SetBlock(int3 worldXYZ, ushort block)
        {
            var chunkIndex = Grid3D.WorldToCell(worldXYZ);
            var chunk = GetOrCreateVoxelChunkFromIndex(chunkIndex);
            ECB.AddComponent<RebuildChunkMesh>(chunk);

            var blocks = _world.BlocksFromEntity[chunk];
            int blockIndex = Grid3D.WorldToIndex(worldXYZ);

            blocks[blockIndex] = block;
        }
        public void SetBlock(int x, int y, int z, ushort block)
        {
            SetBlock(new int3(x, y, z), block);
        }

        public ushort GetBlock(int3 worldXYZ)
        {
            _reader.TryGetBlock(_world, worldXYZ, out var block);
            return block;
        }
        public ushort GetBlock(int x, int y, int z) => GetBlock(new int3(x, y, z));

        public bool TryGetRegionFromIndex(int2 regionIndex, out Entity region)
        {
            return _reader.TryGetRegionFromIndex(_world, regionIndex, out region);
        }
        public bool TryGetRegionFromIndex(int x, int z, out Entity region)
        {
            return TryGetRegionFromIndex(new int2(x, z), out region);
        }

        public bool TryGetVoxelChunkFromIndex(int3 chunkindex, out Entity chunk)
        {
            return _reader.TryGetVoxelChunkFromIndex(_world, chunkindex, out chunk);
        }
        public bool TryGetVoxelChunkFromIndex(int x, int y, int z, out Entity chunk)
        {
            return TryGetVoxelChunkFromIndex(new int3(x, y, z), out chunk);
        }

        public Entity GetVoxelChunkFromIndex(int3 chunkIndex)
        {
            TryGetVoxelChunkFromIndex(chunkIndex, out var chunk);
            return chunk;
        }
        public Entity GetVoxelChunkFromIndex(int x, int y, int z)
        {
            return GetVoxelChunkFromIndex(new int3(x, y, z));
        }

        public NativeArray<ushort> GetOrCreateBlocksArrayFromIndex(int3 chunkIndex)
        {
            if (!_reader.TryGetBlocksArrayFromIndex(_world, chunkIndex, out var blocks))
            {
                var chunk = GetOrCreateVoxelChunkFromIndex(chunkIndex);
                blocks = _world.BlocksFromEntity[chunk].Reinterpret<ushort>().AsNativeArray();
            }

            return blocks;
        }
        public NativeArray<ushort> GetOrCreateBlocksArrayFromIndex(int x, int y, int z)
        {
            return GetOrCreateBlocksArrayFromIndex(new int3(x, y, z));
        }

        public Entity GetOrCreateVoxelChunkFromIndex(int3 chunkIndex)
        {
            if (!_reader.TryGetVoxelChunkFromIndex(_world, chunkIndex, out Entity chunk))
            {
                var region = GetOrCreateRegionFromIndex(chunkIndex.xz);
                chunk = _writer.CreateVoxelChunkFromIndex(_world, chunkIndex, region);
            }
            return chunk;
        }
        public Entity GetOrCreateVoxelChunkFromIndex(int x, int y, int z)
        {
            return GetOrCreateVoxelChunkFromIndex(new int3(x, y, z));
        }

        public Entity GetOrCreateRegionFromIndex(int2 chunkIndex)
        {
            if (!_reader.TryGetRegionFromIndex(_world, chunkIndex, out Entity region))
            {
                region = _writer.CreateRegionFromIndex(_world, chunkIndex);
            }

            return region;
        }
        public Entity GetOrCreateRegionFromIndex(int x, int y )
        {
            return GetOrCreateRegionFromIndex(new int2(x, y));
        }

        public AdjacentChunkBlocks GetAdjacentChunkBlocks(int3 chunkindex)
        {
            return new AdjacentChunkBlocks(_world, chunkindex);
        }
        public AdjacentChunkBlocks GetAdjacentChunkBlocks(int x, int y, int z)
        {
            return GetAdjacentChunkBlocks(new int3(x, y, z));
        }
    }
}