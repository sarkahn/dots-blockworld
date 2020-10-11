using Unity.Entities;
using Unity.Mathematics;

namespace Sark.BlockGame
{
    public struct LoadRegion : IComponentData
    {
        public int2 Index;
    }

    public class LoadRegionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        EntityQuery _regionsToLoad;

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
