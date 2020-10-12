using Sark.Common.CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.BlockGame
{
    public struct VoxelWorldWriter
    {
        public EntityCommandBuffer ECB;

        NativeList<Entity> _regionPool;
        NativeList<Entity> _chunkPool;

        public VoxelWorldWriter(
            EntityCommandBuffer ecb,
            NativeList<Entity> regionPool,
            NativeList<Entity> chunkPool)
        {
            ECB = ecb;
            _regionPool = regionPool;
            _chunkPool = chunkPool;
        }

        public Entity CreateRegionFromIndex(VoxelWorldState world, int2 regionIndex)
        {
            var entity = _regionPool.PopLast();
            ECB.RemoveComponent<Disabled>(entity);

            var regionFromEntity = world.RegionFromEntity;
            var region = regionFromEntity[entity];
            region.Index = regionIndex;
            regionFromEntity[entity] = region;

            var regionMap = world.RegionMap;
            regionMap[regionIndex] = entity;

            return entity;
        }

        public Entity CreateVoxelChunkFromIndex(VoxelWorldState world, int3 chunkIndex, Entity region)
        {
            Entity chunkEntity = _chunkPool.PopLast();
            ECB.RemoveComponent<Disabled>(chunkEntity);

            var chunkFromEntity = world.ChunkFromEntity;
            var chunk = chunkFromEntity[chunkEntity];
            {
                chunk.Index = chunkIndex;
                chunk.Region = region;
            }
            chunkFromEntity[chunkEntity] = chunk;

            var linkedGroup = world.LinkedGroupFromEntity[region];
            linkedGroup.Add(chunkEntity);

            world.ChunkMap[chunkIndex] = chunkEntity;

            return chunkEntity;
        }
    }
}