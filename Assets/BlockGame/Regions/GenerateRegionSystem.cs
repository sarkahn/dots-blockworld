using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class GenerateRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;
        EntityQuery _regionQuery;

        EntityArchetype _chunkArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _chunkArchetype = EntityManager.CreateArchetype(
                typeof(ChunkBlockType)
                );
        }

        protected override void OnUpdate()
        {
            InitializeRegions();

            GenerateHeightMapBlobs();

            GenerateChunks();

            AssignChunkSharedIndices();
        }

        void InitializeRegions()
        {
            var concurrentBuffer = _barrier.CreateCommandBuffer().ToConcurrent();
            // Apply default generation settings if none exist
            Entities
                .WithName("ApplyDefaultRegionGenerationSettings")
                .WithAll<GenerateRegion>()
                .WithNone<GenerateRegionHeightMapSettings>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    concurrentBuffer.AddComponent(entityInQueryIndex, e,
                        GenerateRegionHeightMapSettings.Default);
                }).ScheduleParallel();
            _barrier.AddJobHandleForProducer(Dependency);

            // Assign region indices from "GenerateRegion" components
            // and add "LinkedEntityGroup" buffers for chunks
            var buffer = _barrier.CreateCommandBuffer();
            Entities
                .WithoutBurst()
                .WithName("AssignRegionIndices")
                .WithAll<GenerateRegionHeightMapSettings>()
                .WithNone<RegionIndex>()
                .ForEach((int entityInQueryIndex, Entity e, in GenerateRegion generate) =>
                {
                    int2 i = generate.regionIndex;

                    buffer.AddComponent<RegionIndex>(e, i);
                    buffer.AddSharedComponent<SharedRegionIndex>(e,i);
                    buffer.AddBuffer<LinkedEntityGroup>(e);
                    buffer.AddComponent<Region>(e);
#if UNITY_EDITOR
                    // Note this causes an exception if you generate a lot of regions.
                    //EntityManager.SetName(e, $"Region ({i.x}, {i.y})");
#endif
                }).Run();
            _barrier.AddJobHandleForProducer(Dependency);
        }

        void GenerateHeightMapBlobs()
        {
            var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();
            Entities
                .WithNone<RegionHeightMap>()
                .WithAll<GenerateRegion>()
                .ForEach((int entityInQueryIndex, Entity e, 
                in GenerateRegionHeightMapSettings settings,
                in RegionIndex regionIndex) =>
                {
                    var heightMapBlob = HeightMapBuilder.BuildHeightMap(
                        regionIndex.value * Constants.ChunkSize.xz, Constants.ChunkSurfaceSize,
                        settings, Allocator.Persistent);

                    var blobComponent = new RegionHeightMap { heightMapBlob = heightMapBlob };

                    commandBuffer.AddComponent(entityInQueryIndex, e, blobComponent);

                    commandBuffer.RemoveComponent<GenerateRegionHeightMapSettings>(entityInQueryIndex, e);
                    commandBuffer.RemoveComponent<GenerateRegion>(entityInQueryIndex, e);

                    commandBuffer.AddComponent<GenerateRegionChunks>(entityInQueryIndex, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }

        void GenerateChunks()
        {
            var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();
            var chunkArchetype = _chunkArchetype;
            Entities
                .WithName("GenerateChunksInRegion")
                .WithStoreEntityQueryInField(ref _regionQuery)
                .WithAll<GenerateRegionChunks>()
                .ForEach((Entity e, int entityInQueryIndex,
                ref DynamicBuffer<LinkedEntityGroup> linkedGroup,
                in RegionHeightMap heightMap, in RegionIndex regionIndex
                ) =>
                {
                    ref var hmArray = ref heightMap.Array;
                    int maxHeight = int.MinValue;
                    int maxHeightIndex = -1;

                    for (int i = 0; i < hmArray.Length; ++i)
                    {
                        int height = hmArray[i];
                        if (height > maxHeight)
                        {
                            maxHeight = height;
                            maxHeightIndex = i;
                        }
                    }

                    var playbackBuffer = commandBuffer.SetBuffer<LinkedEntityGroup>(entityInQueryIndex, e);
                    if (linkedGroup.Length > 0)
                        playbackBuffer.AddRange(linkedGroup.AsNativeArray());

                    for (int y = 0; y < maxHeight; y += Constants.ChunkHeight)
                    {
                        var chunkEntity = commandBuffer.CreateEntity(entityInQueryIndex, chunkArchetype);
                        commandBuffer.AddComponent<ChunkWorldHeight>(entityInQueryIndex, chunkEntity, y);
                        commandBuffer.AddComponent<RegionHeightMap>(entityInQueryIndex, chunkEntity, heightMap);
                        commandBuffer.AddComponent<RegionIndex>(entityInQueryIndex, chunkEntity, regionIndex);
                        commandBuffer.AddComponent<GameChunk>(entityInQueryIndex, chunkEntity);
                        commandBuffer.AddComponent<GenerateChunk>(entityInQueryIndex, chunkEntity);

                        playbackBuffer.Add(chunkEntity);
                    }

                    commandBuffer.RemoveComponent<GenerateRegionChunks>(entityInQueryIndex, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }

        void AssignChunkSharedIndices()
        {
            var buffer = _barrier.CreateCommandBuffer();
            Entities
                .WithName("SetChunkSharedIndices")
                .WithoutBurst()
                .WithAll<GameChunk>()
                .WithNone<SharedRegionIndex>()
                .ForEach((Entity e, in RegionIndex regionIndex, in ChunkWorldHeight chunkHeight) =>
                {
                    int2 index = regionIndex.value;
                    buffer.AddSharedComponent<SharedRegionIndex>(e, index);

#if UNITY_EDITOR
                    // Note this causes an exception if you generate a lot of regions.
                    //EntityManager.SetName(e, $"Chunk ({index.x}, {index.y}): {chunkHeight.value / Constants.ChunkHeight}");
#endif
                }).Run();
        }

        // Unusued, switched to using blob assets for height maps
        void GenerateHeightMapBuffer()
        {
            Entities
                .WithName("PopulateHeightmapBuffer")
                .ForEach((ref DynamicBuffer<GenerateRegionHeightMap> heightMapBuffer, 
                in GenerateRegionHeightMapSettings settings,
                in RegionIndex regionIndex) =>
                {
                    heightMapBuffer.ResizeUninitialized(Constants.ChunkSurfaceVolume);
                    var heightMap = heightMapBuffer.Reinterpret<int>().AsNativeArray();

                    int iterations = settings.iterations;
                    int minHeight = settings.minHeight;
                    int maxHeight = settings.maxHeight;
                    float persistence = settings.persistence;
                    float scale = settings.scale;

                    int2 regionWorldPos = regionIndex.value * Constants.ChunkSize.xz;
                    for (int x = 0; x < Constants.ChunkSizeX; ++x)
                        for (int z = 0; z < Constants.ChunkSizeZ; ++z)
                        {
                            int2 localPos = new int2(x, z);
                            int2 worldPos = regionWorldPos + localPos;
                            float h = NoiseUtil.SumOctave(
                                regionIndex.value.x + x, regionIndex.value.y + z,
                                iterations, persistence, scale, minHeight, maxHeight);

                            int i = GridUtil.Grid2D.PosToIndex(localPos);
                            heightMap[i] = (int)math.floor(h);
                        }
                }).ScheduleParallel();
        }
    }
}