using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using GridUtil;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using BlockGame.Chunks;
using System.Globalization;
using Unity.Collections.LowLevel.Unsafe;

namespace BlockGame.Regions
{
    public class GenerateRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;
        EntityQuery _regionsToGenerate;

        EntityArchetype _chunkArchetype;

        MapGenSettingsAsset _genSettings;

        protected override void OnCreate()
        {
            _regionsToGenerate = GetEntityQuery(
                ComponentType.ReadOnly<Region>(),
                ComponentType.ReadOnly<GenerateRegion>(),
                ComponentType.Exclude<HeightMap>()
                );

            _chunkArchetype = EntityManager.CreateArchetype(
                typeof(VoxelChunk),
                typeof(VoxelChunkBlocks)
                );

            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _genSettings = Resources.Load<MapGenSettingsAsset>("MapGenSettings");
        }

        protected override void OnUpdate()
        {
            var ecb = _endSimBarrier.CreateCommandBuffer().AsParallelWriter();

            int chunkSize = 16;

            var genSettings = _genSettings.Settings;
            var chunkArchetype = _chunkArchetype;
            var chunkBuilder = new VoxelChunkBuilder(this);

            Entities
                .WithName("GenerateHeightMap")
                .WithNone<HeightMap>()
                .WithAll<GenerateRegion>()
                .ForEach((int entityInQueryIndex, Entity e, 
                in Region region) =>
                {
                    var map = ecb.AddBuffer<HeightMap>(entityInQueryIndex, e);
                    map.ResizeUninitialized(chunkSize * chunkSize);
                    var arr = map.Reinterpret<ushort>().AsNativeArray();

                    int2 regionIndex = region.Index;
                    int2 origin = regionIndex * chunkSize;

                    BuildMap(
                        arr,
                        chunkSize,
                        origin,
                        genSettings,
                        out int highest);

                    if (highest == 0)
                        return;

                    int maxChunkHeight = highest / Grid3D.CellSizeY;

                    for(int chunkIndexY = 0; chunkIndexY <= maxChunkHeight; ++chunkIndexY )
                    {
                        int3 chunkIndex = new int3(regionIndex.x, chunkIndexY, regionIndex.y);

                        chunkBuilder.CreateVoxelChunk(ecb, entityInQueryIndex, chunkIndex, e);
                    }
                }).ScheduleParallel();

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        static void BuildMap(
            NativeArray<ushort> map, 
            int size, 
            int2 origin,
            MapGenSettings settings,
            out int highest)
        {
            highest = int.MinValue;
            for( int i = 0; i < map.Length; ++i )
            {
                int2 xz = Grid2D.IndexToPos(i);

                int2 p = origin + xz;
                float noise = NoiseUtil.SumOctave(p.x, p.y,
                    settings.Iterations,
                    settings.Persistance,
                    settings.Scale,
                    settings.Low,
                    settings.High);

                map[i] = (ushort)math.floor(noise);
                highest = math.max(highest, map[i]);
            }
        }
    } 
}
