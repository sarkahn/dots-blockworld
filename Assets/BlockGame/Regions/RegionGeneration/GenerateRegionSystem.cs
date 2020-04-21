using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    /// <summary>
    /// A region represents a stack of 1 or more 16x16x16 chunks of blocks in the world. The chunks are connected to each region
    /// via their <see cref="SharedRegionIndex"/> as well as being a part of it's <see cref="LinkedEntityGroup"/>.
    /// </summary>
    public class GenerateRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        EntityArchetype _chunkArchetype;
        EntityQuery _generateHeightMapQuery;


        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _chunkArchetype = EntityManager.CreateArchetype(
                typeof(ChunkBlockBuffer),
                typeof(GameChunk),
                typeof(ChunkWorldHeight),
                typeof(RegionHeightMap),
                typeof(ChunkIndex),
                typeof(GenerateChunk)
                );
        }

        protected override void OnUpdate()
        {
            InitializeRegions();

            GenerateHeightMapBlobs();

            GenerateChunks();

            //AssignChunkSharedIndices();
        }

        void InitializeRegions()
        {
            SetEditorNamesForGeneratedRegions();
        }

        void GenerateHeightMapBlobs()
        {
            GenerateHeightmapSettings settings = GenerateHeightmapSettings.Default;
            var settingsUI = GameObject.FindObjectOfType<MapGenerationSettingsUI>();
            if (settingsUI != null)
                settings = settingsUI.Settings;

            var ecb = _barrier.CreateCommandBuffer().ToConcurrent();
            Entities
                .WithStoreEntityQueryInField(ref _generateHeightMapQuery)
                .WithNone<RegionHeightMap>()
                .WithAll<GenerateRegion>()
                .ForEach((int entityInQueryIndex, Entity e, 
                in RegionIndex regionIndex) =>
                {
                    var heightMapBlob = HeightMapBuilder.BuildHeightMap(
                        regionIndex.value * Constants.ChunkSize.xz, Constants.ChunkSurfaceSize,
                        settings, Allocator.Persistent);

                    var blobComponent = new RegionHeightMap { heightMapBlob = heightMapBlob };

                    ecb.AddComponent(entityInQueryIndex, e, blobComponent);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }

        void GenerateChunks()
        {
            var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();
            var chunkArchetype = _chunkArchetype;
            Entities
                .WithName("GenerateChunksInRegion")
                .WithAll<GenerateRegion>()
                .ForEach((Entity e, int entityInQueryIndex,
                in DynamicBuffer<LinkedEntityGroup> linkedGroup,
                in DynamicBuffer<RegionChunksBuffer> chunkBuffer,
                in RegionHeightMap heightMap, in RegionIndex regionIndex_
                ) =>
                {
                    ref var hmArray = ref heightMap.Array;
                    int maxHeight = int.MinValue;
                    int maxHeightIndex = -1;

                    int2 regionIndex = regionIndex_;

                    for (int i = 0; i < hmArray.Length; ++i)
                    {
                        int height = hmArray[i];
                        if (height > maxHeight)
                        {
                            maxHeight = height;
                            maxHeightIndex = i;
                        }
                    }

                    var linkedGroupBuffer = commandBuffer.SetBuffer<LinkedEntityGroup>(entityInQueryIndex, e);
                    if (linkedGroup.Length > 0)
                        linkedGroupBuffer.AddRange(linkedGroup.AsNativeArray());

                    var chunksBuffer = commandBuffer.SetBuffer<RegionChunksBuffer>(entityInQueryIndex, e);
                    if (chunkBuffer.Length > 0)
                        chunksBuffer.AddRange(chunkBuffer.AsNativeArray());

                    for (int y = 0; y < maxHeight; y += Constants.ChunkHeight)
                    {
                        var chunkEntity = commandBuffer.CreateEntity(entityInQueryIndex, chunkArchetype);
                        commandBuffer.SetComponent<ChunkWorldHeight>(entityInQueryIndex, chunkEntity, y);
                        commandBuffer.SetComponent<RegionHeightMap>(entityInQueryIndex, chunkEntity, heightMap);
                        int3 chunkIndex = new int3(regionIndex.x, y / Constants.ChunkHeight, regionIndex.y);
                        commandBuffer.SetComponent<ChunkIndex>(entityInQueryIndex, chunkEntity, chunkIndex);
                        commandBuffer.SetComponent<GameChunk>(entityInQueryIndex, chunkEntity, new GameChunk { parentRegion = e });

                        linkedGroupBuffer.Add(chunkEntity);
                        chunksBuffer.Add(chunkEntity);
                    }

                    commandBuffer.RemoveComponent<GenerateRegion>(entityInQueryIndex, e);
                }).ScheduleParallel();

            Entities.WithAll<GenerateChunk>()
                .WithoutBurst()
                .ForEach((Entity e,in ChunkIndex chunkIndex) =>
                {
                    int3 i = chunkIndex;
#if UNITY_EDITOR
                    // Note this causes an exception if you generate a lot of regions.
                    EntityManager.SetName(e, $"Chunk ({i.x}, {i.z}: {i.y})");
#endif
                }).Run();

            _barrier.AddJobHandleForProducer(Dependency);
        }

        // Unusued, switched to using blob assets for height maps
        void GenerateHeightMapBuffer()
        {
            Entities
                .WithName("PopulateHeightmapBuffer")
                .ForEach((ref DynamicBuffer<GenerateRegionHeightMap> heightMapBuffer, 
                in GenerateHeightmapSettings settings,
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

        [Conditional("UNITY_EDITOR")]
        void SetEditorNamesForGeneratedRegions()
        {
#if UNITY_EDITOR
            Entities.WithoutBurst().WithAll<GenerateRegion>().ForEach((Entity e, RegionIndex regionIndex) =>
            {
                var i = regionIndex.value;
                // Note this causes an exception if you generate a lot of regions.
                EntityManager.SetName(e, $"Region ({i.x}, {i.y})");
            }).Run();
#endif
        }
    }
}