using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sark.BlockGame
{
    public struct RegionLoaded : IComponentData { }
    public struct UnloadRegion : IComponentData { }

    public class RegionLoaderSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimSystem;

        NativeHashSet<int2> _pointSet;

        NativeList<int2> _toLoadPoints;
        NativeList<Entity> _loadedRegions;

        int _chunkSize = 16;

        EntityQuery _regionLoaderQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            _endSimSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _pointSet = new NativeHashSet<int2>(500, Allocator.Persistent);

            _loadedRegions = new NativeList<Entity>(500, Allocator.Persistent);
            _toLoadPoints = new NativeList<int2>(500, Allocator.Persistent);

            RequireForUpdate(_regionLoaderQuery);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _pointSet.Dispose(Dependency);
            _loadedRegions.Dispose(Dependency);
            _toLoadPoints.Dispose(Dependency);
        }

        // TODO: Account for region count larger than "loadedPoints"
        // map. Foreach loader: area = (range * 2 + 1) * (range * 2 + 1)
        protected override void OnUpdate()
        {
            SetEditorNames();

            if (_regionLoaderQuery.CalculateEntityCount() == 0)
                return;

            int chunkSize = _chunkSize;
            var pointSet = _pointSet;
            var loadedRegions = _loadedRegions;
            var pointsToLoad = _toLoadPoints;
            var ecb = _endSimSystem.CreateCommandBuffer();

            var regionPrefab = World.GetOrCreateSystem<VoxelWorldSystem>().GetRegionPrefab();

            Job.WithCode(() =>
            {
                pointSet.Clear();
                pointsToLoad.Clear();
                loadedRegions.Clear();
            }).Schedule();

            // Get all currently loaded/loading regions
            Entities
                .WithAll<Region>()
                .WithAny<RegionLoaded, LoadRegion>()
                .ForEach((Entity e) =>
            {
                loadedRegions.Add(e);
            }).Schedule();

            var pointWriter = pointsToLoad.AsParallelWriter();
            Entities
                .WithName("GetAllPointsInRangeOfLoaders")
                .WithChangeFilter<RegionLoader>()
                .WithChangeFilter<Translation>()
                .WithStoreEntityQueryInField(ref _regionLoaderQuery)
                .ForEach((in RegionLoader loader, in Translation translation) =>
                {
                    var loaderPos = translation.Value;
                    int3 loaderCell = (int3)math.floor(loaderPos / chunkSize);
                    int range = loader.Range;

                    if (range < 0)
                        return;
                    else if (range == 0)
                    {
                        pointWriter.AddNoResize(loaderCell.xz);
                        return;
                    }

                    for (int x = -range; x <= range; ++x)
                    {
                        for (int z = -range; z <= range; ++z)
                        {
                            pointWriter.AddNoResize(loaderCell.xz + new int2(x, z));
                        }
                    }
                }).ScheduleParallel();

            Job.WithName("PrepareRegionsForLoadingAndUnloading").WithCode(() =>
            {
                // Add all points which must be loaded to our set
                for (int i = 0; i < pointsToLoad.Length; ++i)
                        pointSet.Add(pointsToLoad[i]);

                // Remove all points which are already loading/loaded from our set...
                for (int i = loadedRegions.Length - 1; i >= 0; --i)
                {
                    var loadedIndex = GetComponent<Region>(loadedRegions[i]).Index;
                    if (pointSet.Contains(loadedIndex))
                    {
                        pointSet.Remove(loadedIndex);
                        // And remove them from the list as well
                        loadedRegions.RemoveAt(i);
                    }
                }

                // Any remaining points in this list are no longer
                // in range of a region loader and must be unloaded
                // TODO: This should be handled in a separate system
                // that would unload the regions to disk or memory or whatever
                for (int i = 0; i < loadedRegions.Length; ++i)
                        ecb.DestroyEntity(loadedRegions[i]);

                // Whatever remains in the pointset needs to be loaded
                var toLoad = pointSet.ToNativeArray(Allocator.Temp);

                for (int i = 0; i < toLoad.Length; ++i)
                {
                    var regionIndex = toLoad[i];
                    float3 worldPos = new float3();
                    worldPos.xz = regionIndex * chunkSize;

                    var region = ecb.Instantiate(regionPrefab);
                    ecb.SetComponent(region,
                        new Region { Index = regionIndex });
                    ecb.AddComponent<LoadRegion>(region);
                }
            }).Schedule();

            _endSimSystem.AddJobHandleForProducer(Dependency);
        }

        struct NamedInEditor : IComponentData { }

        [Conditional("UNITY_EDITOR")]
        void SetEditorNames()
        {
#if UNITY_EDITOR

            Entities.WithStructuralChanges()
                .WithNone<NamedInEditor>()
                .ForEach((Entity e, in Region region) =>
            {
                var idx = region.Index;
                EntityManager.SetName(
                    e, 
                    $"Region ({idx.x},{idx.y})");
                EntityManager.AddComponent<NamedInEditor>(e);
            }).Run();
#endif
        }
    }

}