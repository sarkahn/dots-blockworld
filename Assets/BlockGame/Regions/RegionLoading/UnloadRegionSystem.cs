using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class UnloadRegionSystem : SystemBase
    {
        EndInitializationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer().ToConcurrent();

            Entities.WithAll<UnloadRegion>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    ecb.DestroyEntity(entityInQueryIndex, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

}