using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.Regions
{
    public class LoadRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        EntityQuery _regionsToLoad;
        //EntityQuery _regionsDoneGenerating;

        protected override void OnCreate()
        {
            base.OnCreate();

            _regionsToLoad = GetEntityQuery(
                ComponentType.ReadOnly<LoadRegion>(),
                ComponentType.Exclude<GenerateRegion>()
                );

            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endSimBarrier.CreateCommandBuffer();

            // TODO : Check if regions were previously loaded and restore from disk/memory

            ecb.AddComponent<GenerateRegion>(_regionsToLoad);
            ecb.RemoveComponent<LoadRegion>(_regionsToLoad);

            //ecb.RemoveComponent<LoadRegion>(_regionsDoneGenerating);
            //ecb.RemoveComponent<GenerateRegion>(_regionsDoneGenerating);
            //ecb.AddComponent<RegionLoaded>(_regionsDoneGenerating);

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
