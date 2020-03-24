using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using Unity.Profiling;

namespace BlockGame.BlockWorld
{
    public class GenerateChunkSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        struct BlockIDs
        {
            public int sand;
            public int dirt;
            public int grass;
            public int stone;
        }

        BlockIDs GetBlockIDs()
        {
            var ids = new BlockIDs();
            var registry = World.GetOrCreateSystem<BlockRegistrySystem>();
            ids.dirt = registry.GetBlockID("Dirt");
            ids.sand = registry.GetBlockID("Sand");
            ids.grass = registry.GetBlockID("Grass");
            ids.stone = registry.GetBlockID("Stone");
            return ids;
        }

        static ChunkBlockType SelectBlock(int height, int maxHeight, ref BlockIDs ids)
        {
            ChunkBlockType block = default;

            int sandHeight = 4;
            int dirtHeight = 7;
            int grassHeight = 10;
            int stonHeight = int.MaxValue;

            // Sand
            if (height <= sandHeight)
                block = ids.sand;
            // Dirt
            else if (height <= dirtHeight )
                block = ids.dirt;
            // Grass
            else if (height <= grassHeight )
            {
                if (height == maxHeight)
                    block = ids.grass;
                else
                    block = ids.dirt;
            }
            // Stone
            else if (height < stonHeight )
                block = ids.stone;

            return block;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();

            Entities
                .WithName("AddBlockBuffersToChunks")
                .WithAll<GenerateChunk>()
                .WithNone<ChunkBlockType>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    commandBuffer.AddBuffer<ChunkBlockType>(entityInQueryIndex, e);
                }).ScheduleParallel();

            var blockIDs = GetBlockIDs();

            ProfilerMarker marker = new ProfilerMarker("InitChunkBlocks");
            Entities
                .WithName("InitializeChunkBlocks")
                .WithAll<GenerateChunk>()
                .ForEach((int entityInQueryIndex, Entity e, ref DynamicBuffer<ChunkBlockType> blocksBuffer, 
                in ChunkWorldHeight chunkWorldHeight, in RegionHeightMap heightMapBlob, in RegionIndex regionIndex) =>
                {
                    blocksBuffer.ResizeUninitialized(Constants.ChunkVolume);

                    var blocks = blocksBuffer.AsNativeArray();

                    ref var heightMap = ref heightMapBlob.Array;

                    for( int i = 0; i < blocks.Length; ++i )
                    {
                        int3 xyz = GridUtil.Grid3D.IndexToPos(i);
                        int x = xyz.x;
                        int y = xyz.y;
                        int z = xyz.z;

                        int xzIndex = GridUtil.Grid2D.PosToIndex(x, z);

                        int maxHeight = heightMap[xzIndex];
                        int height = y + chunkWorldHeight;

                        if (height <= maxHeight)
                            blocks[i] = SelectBlock(height, maxHeight, ref blockIDs);
                        else
                            blocks[i] = default;
                    }

                    commandBuffer.RemoveComponent<GenerateChunk>(entityInQueryIndex, e);
                    commandBuffer.AddComponent<GenerateMesh>(entityInQueryIndex, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}