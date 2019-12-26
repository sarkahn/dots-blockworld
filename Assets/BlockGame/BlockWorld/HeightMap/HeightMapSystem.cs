using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace BlockWorld
{

    public class HeightMapSystem : JobComponentSystem
    {
        BeginSimulationEntityCommandBufferSystem _ecbSystem;

        EntityQuery _generationSettingsQuery;

        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            HeightMapGenerationSettings settings = HeightMapGenerationSettings.Default;

            Entities
                .WithoutBurst()
                .ForEach((HeightMapGenerationSettingsAuthoring authoringSettings) =>
                {
                // Copy settings from runtime component if it exists
                    settings.Iterations = authoringSettings.Iterations;
                    settings.Persistence = authoringSettings.Persistence;
                    settings.Scale = authoringSettings.Scale;
                    settings.High = authoringSettings.High;
                    settings.Low = authoringSettings.Low;
                }).Run();

            var commandBuffer = _ecbSystem.CreateCommandBuffer().ToConcurrent();

            inputDeps = Entities
                .WithAll<GenerateHeightMap>()
                .WithNone<HeightMapBuffer>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    var b = commandBuffer.AddBuffer<HeightMapBuffer>(entityInQueryIndex, e);
                    b.ResizeUninitialized(Constants.Regions.Volume);
                }).Schedule(inputDeps);

            int2 regionSize = Constants.Regions.Size;

            inputDeps = Entities
                .ForEach((
                    int entityInQueryIndex, 
                    Entity e, 
                    ref DynamicBuffer<HeightMapBuffer> heightMapBuffer, 
                    in GenerateHeightMap gen) =>
                {
                    int2 regionIndex = gen.regionIndex;
                    int2 regionWorldPos = (int2)GridMath.Grid2D.WorldPosFromCellIndex(regionIndex, regionSize).xz;

                    for ( int x = 0; x < Constants.Regions.SizeX; ++x )
                    {
                        for( int y = 0; y < Constants.Regions.SizeZ; ++y )
                        {
                            int2 cellPos = new int2(x, y);
                            int index = GridMath.Grid2D.ArrayIndexFromCellPos(cellPos, regionSize);
                            int2 worldPos = regionWorldPos + cellPos;

                            heightMapBuffer[index] = SumOctave(
                                settings.Iterations, 
                                worldPos.x, 
                                worldPos.y, 
                                settings.Persistence, 
                                settings.Scale, 
                                settings.Low, 
                                settings.High);
                        }
                    }

                    commandBuffer.RemoveComponent<GenerateHeightMap>(entityInQueryIndex, e);
                }).Schedule(inputDeps);

            _ecbSystem.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float SumOctave(int iterations, int x, int y, float persistence, float scale, int low, int high)
        {
            float maxAmp = 0;
            float amp = 1;
            float freq = scale;
            float v = 0;

            for (int i = 0; i < iterations; ++i)
            {
                v += noise.snoise(new float2(x * freq, y * freq)) * amp;
                maxAmp += amp;
                amp *= persistence;
                freq *= 2;
            }

            v /= maxAmp;

            v = v * (high - low) / 2f + (high + low) / 2f;

            return v;
        }
    }

}