using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(GenerateRegionSystem))]
    public class RegionNeighboursSystem : SystemBase
    {
        EntityQuery _updateNeighboursQuery;
        EntityQuery _regionQuery;

        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();
            _regionQuery = GetEntityQuery(
                ComponentType.ReadOnly<SharedRegionIndex>(),
                ComponentType.ReadOnly<Region>()
                );
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();

            // Add an "UpdateNeighbours" component to any generating region and to all generating region's neighbours
            Entities
                .WithName("AddNeighbourUpdateComponent")
                .WithNone<UpdateRegionNeighbours>()
                .WithAll<GenerateRegion>()
                .WithAll<Region>()
                .WithoutBurst()
                .ForEach((Entity e, SharedRegionIndex sharedIndex) =>
                {
                    ecb.AddComponent<UpdateRegionNeighbours>(e, sharedIndex.value);

                    for( int dirIndex = 0; dirIndex < GridUtil.Grid2D.Directions.Length; ++dirIndex)
                    {
                        var dir = GridUtil.Grid2D.Directions[dirIndex];
                        var adjPos = sharedIndex.value + dir;

                        _regionQuery.SetSharedComponentFilter<SharedRegionIndex>(adjPos);
                        if (_regionQuery.CalculateEntityCount() == 0)
                            continue;


                        var adj = _regionQuery.GetSingletonEntity();
                        
                        ecb.AddComponent<UpdateRegionNeighbours>(adj, new UpdateRegionNeighbours { regionIndex = adjPos });
                    }
                }).Run();

            Entities
                .WithoutBurst()
                .WithStoreEntityQueryInField(ref _updateNeighboursQuery)
                .ForEach((Entity e, SharedRegionIndex regionIndex, UpdateRegionNeighbours update, DynamicBuffer<RegionNeighbours> neighbours) =>
                {
                    for (int dirIndex = 0; dirIndex < GridUtil.Grid2D.Directions.Length; ++dirIndex)
                    {
                        var dir = GridUtil.Grid2D.Directions[dirIndex];
                        var adjPos = update.regionIndex + dir;

                        _regionQuery.SetSharedComponentFilter<SharedRegionIndex>(adjPos);

                        if (_regionQuery.CalculateEntityCount() == 0)
                            continue;

                        var adj = _regionQuery.GetSingletonEntity();

                        for ( int i = 0; i < neighbours.Length; ++i )
                        {
                            if (neighbours[i] == adj)
                                return;
                        }

                        neighbours[dirIndex] = adj;
                    }

                    ecb.RemoveComponent<UpdateRegionNeighbours>(e);
                }).Run();   

            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
