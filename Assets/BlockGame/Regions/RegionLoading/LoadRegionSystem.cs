using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class LoadRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer().ToConcurrent();

            Entities
                .WithAll<LoadRegion>()
                .ForEach((int entityInQueryIndex, Entity e, in RegionIndex regionIndex) =>
                {
                    //ecb.DestroyEntity(entityInQueryIndex, e);
                    //var region = ecb.CreateEntity(entityInQueryIndex, regionArchetype);
                    //ecb.SetComponent<RegionIndex>(entityInQueryIndex, region, regionIndex);
                    ecb.AddComponent<Region>(entityInQueryIndex, e);
                    ecb.AddBuffer<RegionChunksBuffer>(entityInQueryIndex, e);
                    
                    // Note for LinkedEntityGroup to work properly the "parent" should be the first element
                    var linkedGroup = ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, e);
                    linkedGroup.Add(e);

                    // TODO: Try to load the region from disk/memory first. If that fails, THEN
                    // add the GenerateRegion component to let GenerateRegionSystem do it's thing.
                    ecb.AddComponent<GenerateRegion>(entityInQueryIndex, e);

                    ecb.RemoveComponent<LoadRegion>(entityInQueryIndex, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}