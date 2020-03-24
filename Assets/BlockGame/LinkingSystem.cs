using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(RegionLoaderSystem))]
    public class LinkingSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = _barrier.CreateCommandBuffer();
            var linkedGroupFromEntity = GetBufferFromEntity<LinkedEntityGroup>(false);

            Entities
                .WithAll<Disabled>()
                .ForEach((int entityInQueryIndex, Entity e, in LinkToEntity link) =>
                {
                    if (link.target == Entity.Null || !linkedGroupFromEntity.Exists(link.target))
                        return;
                    DynamicBuffer<LinkedEntityGroup> buffer = linkedGroupFromEntity[link.target];

                    buffer.Add(e);

                    commandBuffer.RemoveComponent<LinkToEntity>(e);
                    commandBuffer.RemoveComponent<Disabled>(e);
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
