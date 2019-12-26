using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Responsible for generating a region of chunks in a 16x16 area of the world.
/// A chunk region is a vertical section of an arbitrary number of chunks.
/// </summary>
public class RegionSystem : JobComponentSystem
{
    BeginSimulationEntityCommandBufferSystem _bufferSystem;

    EntityQuery _generateChunksQuery;

    NativeHashMap<int2, Entity> _regionMap;

    protected override void OnCreate()
    {
        _bufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    
    public void GetRegion()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = _bufferSystem.CreateCommandBuffer();
        var concurrentBuffer = commandBuffer.ToConcurrent();

        inputDeps = Entities
            .WithAll<GenerateRegion>()
            .WithStoreEntityQueryInField(ref _generateChunksQuery)
            .ForEach((int entityInQueryIndex, Entity e) =>
            {
                concurrentBuffer.RemoveComponent<GenerateRegion>(entityInQueryIndex, e);
                concurrentBuffer.AddComponent<GeneratingRegion>(entityInQueryIndex, e);
                concurrentBuffer.AddComponent<GenerateHeightMap>(entityInQueryIndex, e);
            }).Schedule(inputDeps);
        
        _bufferSystem.AddJobHandleForProducer(inputDeps);

        return inputDeps;
    }
}
