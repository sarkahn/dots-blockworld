using UnityEngine;
using System.Collections;
using Unity.Entities;
using BlockGame.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using GridUtil;
using BlockGame.CollectionExtensions;
using System.Runtime.InteropServices;
using BlockGame.Regions;

namespace BlockGame.VoxelWorld
{
    public partial class VoxelWorldSystem : SystemBase
    {
        public struct VoxelWorld
        {
            VoxelWorldAccessor _accessor;

            EntityCommandBuffer _ecb;

            NativeList<Entity> _regionPool;
            NativeList<Entity> _chunkPool;

            public VoxelWorld(VoxelWorldSystem system)
            {
                _accessor = new VoxelWorldAccessor(system, false);
                _ecb = system._endSimBarrier.CreateCommandBuffer();

                _regionPool = system._regionPool;
                _chunkPool = system._chunkPool;
            }

            #region SINGLEBLOCK
            public void SetBlock(int3 worldXYZ, ushort block)
            {
                var chunkIndex = Grid3D.WorldToCell(worldXYZ);
                var blocks = GetOrCreateBlocksArrayFromIndex(chunkIndex);
                int blockIndex = Grid3D.WorldToIndex(worldXYZ);

                blocks[blockIndex] = block;
            }
            public void SetBlock(int x, int y, int z, ushort block)
            {
                SetBlock(new int3(x, y, z), block);
            }

            public ushort GetBlock(int3 worldXYZ)
            {
                TryGetBlockInternal(_accessor, worldXYZ, out ushort block);
                return block;
            }
            public ushort GetBlock(int x, int y, int z) => GetBlock(new int3(x, y, z));
            #endregion

            #region BLOCKARRAYS
            public NativeArray<ushort> GetOrCreateBlocksArrayFromIndex(int3 chunkIndex)
            {
                if(!TryGetBlocksArrayFromIndexInternal(_accessor, chunkIndex, out var blocks))
                {
                    var chunk = GetOrCreateVoxelChunkFromIndex(chunkIndex);
                    blocks = _accessor.BlocksFromEntity[chunk].Reinterpret<ushort>().AsNativeArray();
                }

                return blocks;
            }
            public NativeArray<ushort> GetOrCreateBlocksArrayFromIndex(int x, int y, int z)
            {
                return GetOrCreateBlocksArrayFromIndex(new int3(x, y, z));
            }
            #endregion

            #region CHUNKS
            public Entity CreateVoxelChunkFromIndex(int3 chunkIndex, Entity region)
            {
                var chunkStack = _accessor.ChunkStackFromEntity[region];
                int stackIndex = chunkIndex.y;

                // No VoxelChunk has been created at or above the given height
                while (chunkIndex.y >= chunkStack.Length)
                {
                    chunkStack.Add(default);
                }

                Entity chunkEntity = chunkStack[stackIndex];
                if (chunkEntity == Entity.Null)
                {
                    chunkEntity = _chunkPool.PopLast();
                    _ecb.RemoveComponent<Disabled>(chunkEntity);

                    chunkStack[stackIndex] = chunkEntity;

                    var chunkFromEntity = _accessor.ChunkFromEntity;
                    var chunk = chunkFromEntity[chunkEntity];
                    {
                        chunk.Index = chunkIndex;
                        chunk.Region = region;
                    }
                    chunkFromEntity[chunkEntity] = chunk;

                    var linkedGroup = _accessor.LinkedGroupFromEntity[region];
                    linkedGroup.Add(chunkEntity);
                }

                return chunkEntity;
            }

            public Entity GetOrCreateVoxelChunkFromIndex(int3 chunkIndex)
            {
                if(!TryGetVoxelChunkFromIndexInternal(_accessor, chunkIndex, out Entity chunk))
                {
                    var region = GetOrCreateRegionFromIndex(chunkIndex.xz);
                    chunk = CreateVoxelChunkFromIndex(chunkIndex, region);
                }
                return chunk;
            }
            public Entity GetOrCreateVoxelChunkFromIndex(int x, int y, int z)
            {
                return GetOrCreateVoxelChunkFromIndex(new int3(x, y, z));
            }
            #endregion

            #region REGIONS
            public Entity CreateRegionFromIndex(int2 regionIndex)
            {
                var entity = _regionPool.PopLast();
                _ecb.RemoveComponent<Disabled>(entity);

                var regionFromEntity = _accessor.RegionFromEntity;
                var region = regionFromEntity[entity];
                region.Index = regionIndex;
                regionFromEntity[entity] = region;

                var regionMap = _accessor.RegionMap;
                regionMap[regionIndex] = entity;

                return entity;
            }

            public Entity GetOrCreateRegionFromIndex(int2 chunkIndex)
            {
                if(!TryGetRegionFromIndexInternal(_accessor, chunkIndex, out Entity region))
                {
                    region = CreateRegionFromIndex(chunkIndex);
                }

                return region;
            }
            public Entity GetOrCreateRegionFromIndex(int x, int y)
            {
                return GetOrCreateRegionFromIndex(new int2(x, y));
            }
            #endregion;

        }

        /// <summary>
        /// Provides read-only random access to the voxel world. Can be used in parallel jobs.
        /// Should be obtained from <see cref="VoxelWorldSystem.GetVoxelWorldReadOnly"/>.
        /// </summary>
        public struct VoxelWorldReadOnly
        {
            VoxelWorldAccessor _accessor;

            public VoxelWorldReadOnly(VoxelWorldSystem system)
            {
                _accessor = new VoxelWorldAccessor(system, true);
            }

            public ushort GetBlock(int3 worldPos)
            {
                return GetBlockInternal(_accessor, worldPos);
            }

            public ushort GetBlock(int x, int y, int z)
            {
                return GetBlockInternal(_accessor, new int3(x, y, z));
            }

            public bool TryGetBlock(int3 worldPos, out ushort block)
            {
                return TryGetBlockInternal(_accessor, worldPos, out block);
            }

            public bool TryGetRegionFromIndex(int2 regionIndex, out Entity region)
            {
                return TryGetRegionFromIndexInternal(_accessor, regionIndex, out region);
            }

            public bool TryGetBlocksArrayFromIndex(
                int3 chunkIndex,
                out NativeArray<ushort> blocks)
            {
                return TryGetBlocksArrayFromIndexInternal(_accessor, chunkIndex, out blocks);
            }

            public bool TryGetVoxelChunkFromIndex(int3 chunkIndex, out Entity chunkEntity)
            {
                return TryGetVoxelChunkFromIndexInternal(_accessor, chunkIndex, out chunkEntity);
            }
        }

        static ushort GetBlockInternal(VoxelWorldAccessor accessor, int3 worldPos)
        {
            TryGetBlockInternal(accessor, worldPos, out ushort block);
            return block;
        }

        static bool TryGetBlockInternal(VoxelWorldAccessor accessor, int3 worldPos, out ushort block)
        {
            int3 chunkIndex = Grid3D.WorldToCell(worldPos);
            block = 0;
            if (!TryGetBlocksArrayFromIndexInternal(accessor, chunkIndex, out var blocks))
                return false;

            int blockIndex = Grid3D.WorldToIndex(worldPos);
            block = blocks[blockIndex];
            return true;
        }

        static bool TryGetRegionFromIndexInternal(VoxelWorldAccessor accessor, int2 regionIndex, out Entity region)
        {
            return accessor.RegionMap.TryGetValue(regionIndex, out region);
        }

        static bool TryGetBlocksArrayFromIndexInternal(
            VoxelWorldAccessor accessor,
            int3 chunkIndex,
            out NativeArray<ushort> blocks)
        {
            blocks = default;

            if (!TryGetRegionFromIndexInternal(accessor, chunkIndex.xz, out Entity region))
                return false;

            if (!TryGetVoxelChunkFromIndexInternal(accessor, chunkIndex, out Entity chunkEntity))
                return false;

            var blocksBuffer = accessor.BlocksFromEntity[chunkEntity];

            blocks = blocksBuffer.Reinterpret<ushort>().AsNativeArray();

            return true;
        }

        static bool TryGetVoxelChunkFromIndexInternal(
            VoxelWorldAccessor accessor,
            int3 chunkIndex, 
            out Entity chunkEntity)
        {
            chunkEntity = Entity.Null;

            if (!TryGetRegionFromIndexInternal(accessor, chunkIndex.xz, out Entity region))
                return false;

            var chunkStack = accessor.ChunkStackFromEntity[region];
            int stackIndex = chunkIndex.y;

            if (chunkIndex.y >= chunkStack.Length)
                return false;

            chunkEntity = chunkStack[stackIndex];

            return chunkEntity != Entity.Null;
        }

        public struct VoxelWorldAccessor
        {
            public BufferFromEntity<VoxelChunkStack> ChunkStackFromEntity;
            public BufferFromEntity<VoxelChunkBlocks> BlocksFromEntity;
            public BufferFromEntity<LinkedEntityGroup> LinkedGroupFromEntity;

            public ComponentDataFromEntity<Region> RegionFromEntity;
            public ComponentDataFromEntity<VoxelChunk> ChunkFromEntity;

            public NativeHashMap<int2, Entity> RegionMap;

            public VoxelWorldAccessor(VoxelWorldSystem system, bool readOnly = false)
            {
                RegionMap = system._regionMap;

                ChunkStackFromEntity = system.GetBufferFromEntity<VoxelChunkStack>(readOnly);
                BlocksFromEntity = system.GetBufferFromEntity<VoxelChunkBlocks>(readOnly);
                LinkedGroupFromEntity = system.GetBufferFromEntity<LinkedEntityGroup>(readOnly);

                ChunkFromEntity = system.GetComponentDataFromEntity<VoxelChunk>(readOnly);
                RegionFromEntity = system.GetComponentDataFromEntity<Region>(readOnly);
            }
        }
    }
}