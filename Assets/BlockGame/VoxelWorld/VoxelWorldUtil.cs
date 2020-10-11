using BlockGame.Chunks;
using BlockGame.CollectionExtensions;
using BlockGame.VoxelWorldNS;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class VoxelWorldUtil : MonoBehaviour
{
    public static Entity AddChunkToRegion(
        EntityCommandBuffer.ParallelWriter ecb,
        int entityInQueryIndex,
        int3 chunkIndex,
        Entity chunkPrefab,
        Entity region )
    {
        var chunk = ecb.Instantiate(entityInQueryIndex, chunkPrefab);

        ecb.SetComponent(entityInQueryIndex, chunk, new VoxelChunk
        {
            Index = chunkIndex,
            Region = region
        });

        ecb.AppendToBuffer<LinkedEntityGroup>(entityInQueryIndex, region, chunk);

        return chunk;
    }

    public static Entity AddChunkToRegion(
        VoxelWorldSystem.VoxelWorldAccessor world,
        EntityCommandBuffer ecb,
        NativeList<Entity> chunkPool,
        int3 chunkIndex,
        Entity region )
    {
        Entity chunkEntity = chunkPool.PopLast();
        ecb.RemoveComponent<Disabled>(chunkEntity);

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
