using Unity.Entities;
using BlockGame.Chunks;
using Unity.Collections;
using BlockGame.Regions;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Assertions;
using System;

namespace BlockGame.VoxelWorldNS
{
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class VoxelWorldSystem : SystemBase
    {
        NativeHashMap<int2, Entity> _regionMap;
        NativeHashMap<int3, Entity> _chunkMap;

        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        NativeList<Entity> _regionPool;
        NativeList<Entity> _chunkPool;

        EntityStagingHelper _regionBuilder;
        EntityStagingHelper _chunkBuilder;

        EntityQuery _destroyedChunks;
        EntityQuery _destroyedRegions;

        WorldEntityPrefabLoader _prefabLoader;

        public int MinimumPoolSize { get; set; }

        int RegionPoolRefillAmount => MinimumPoolSize * 2;
        int ChunkPoolRefillAmount => MinimumPoolSize * 5;

        public JobHandle GetOutputDependency() => Dependency;

        public void AddInputDependency(JobHandle inputDep) =>
            Dependency = JobHandle.CombineDependencies(Dependency, inputDep);

        protected override void OnCreate()
        {
            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _regionMap = new NativeHashMap<int2, Entity>(10000, Allocator.Persistent);
            _chunkMap = new NativeHashMap<int3, Entity>(50000, Allocator.Persistent);

            _regionPool = new NativeList<Entity>(1000, Allocator.Persistent);
            _chunkPool = new NativeList<Entity>(5000, Allocator.Persistent);

            _chunkBuilder = new EntityStagingHelper("ChunkBuilder");
            _regionBuilder = new EntityStagingHelper("RegionBuilder");

            MinimumPoolSize = 100;

            _prefabLoader = new WorldEntityPrefabLoader(EntityManager);
        }

        protected override void OnDestroy()
        {
            _regionMap.Dispose(Dependency);
            _regionPool.Dispose(Dependency);
            _chunkPool.Dispose(Dependency);
            _chunkMap.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            SetupPools();

            var regionMap = _regionMap;
            var chunkMap = _chunkMap;
            var regionsBuffer = _endSimBarrier.CreateCommandBuffer().AsParallelWriter();
            var chunksBuffer = _endSimBarrier.CreateCommandBuffer().AsParallelWriter();

            var mapRegions = Entities
                .WithNone<MappedRegion>()
                .ForEach((int entityInQueryIndex, Entity e, in Region region) =>
                {
                    regionMap[region.Index] = e;
                    regionsBuffer.AddComponent(entityInQueryIndex, e, 
                        new MappedRegion { Index = region.Index });
                }).Schedule(Dependency);


            var mapChunks = Entities.WithStoreEntityQueryInField(ref _destroyedChunks)
                .WithNone<MappedRegion>()
                .ForEach((int entityInQueryIndex, Entity e, in VoxelChunk chunk) =>
                {
                    chunkMap[chunk.Index] = e;
                    chunksBuffer.AddComponent(entityInQueryIndex, e, 
                        new MappedChunk { Index = chunk.Index });
                }).Schedule(Dependency);

            Dependency = JobHandle.CombineDependencies(mapRegions, mapChunks);

            var clearRegions = Entities
                .WithStoreEntityQueryInField(ref _destroyedRegions)
                .WithNone<Region>()
                .ForEach((in MappedRegion mapped) =>
                {
                    regionMap.Remove(mapped.Index);
                }).Schedule(Dependency);

            var clearChunks = Entities
                .WithStoreEntityQueryInField(ref _destroyedChunks)
                .WithNone<VoxelChunk>()
                .ForEach((in MappedChunk mapped) =>
                {
                    chunkMap.Remove(mapped.Index);
                }).Schedule(Dependency);

            Dependency = JobHandle.CombineDependencies(clearChunks, clearRegions);

            var ecb = _endSimBarrier.CreateCommandBuffer();
            ecb.RemoveComponent<MappedRegion>(_destroyedRegions);
            ecb.RemoveComponent<MappedChunk>(_destroyedChunks);

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        void SetupPools()
        {
            if (!_regionBuilder.IsRunning && _regionPool.Length < MinimumPoolSize)
            {
                _regionBuilder.ScheduleCreateRegionsJob(RegionPoolRefillAmount);
            }
            else if (_regionBuilder.IsRunning && _regionBuilder.IsCompleted)
                CompleteRegionBuilderJob();

            if (!_chunkBuilder.IsRunning && _chunkPool.Length < MinimumPoolSize)
            {
                _chunkBuilder.ScheduleCreateChunksJob(ChunkPoolRefillAmount);
            }
            else if (_chunkBuilder.IsRunning && _chunkBuilder.IsCompleted)
                CompleteChunkBuilderJob();
        }

        void CompleteRegionBuilderJob()
        {
            var arr = _regionBuilder.Complete(World);
            _regionPool.AddRange(arr);
            arr.Dispose();
        }

        void CompleteChunkBuilderJob()
        {
            var arr = _chunkBuilder.Complete(World);
            _chunkPool.AddRange(arr);
            arr.Dispose();
        }

        public void SetupPoolsNow()
        {
            if (_regionBuilder.IsRunning)
                CompleteRegionBuilderJob();
            
            if (_regionPool.Length < MinimumPoolSize)
            {
                _regionBuilder.ScheduleCreateRegionsJob(RegionPoolRefillAmount);
                CompleteRegionBuilderJob();
            }

            if (_chunkBuilder.IsRunning)
                CompleteChunkBuilderJob();

            if(_chunkPool.Length < MinimumPoolSize)
            {
                _chunkBuilder.ScheduleCreateChunksJob(ChunkPoolRefillAmount);
                CompleteChunkBuilderJob();
            }
        }

        public VoxelWorld GetVoxelWorld()
        {
            if(MinimumPoolSize == 0 && (_chunkPool.Length == 0 || _regionPool.Length == 0))
            {
                throw new InvalidOperationException("Error initializing voxel world - minimum pool size is not set");
            }

            SetupPoolsNow();

            Assert.IsTrue(_chunkPool.Length >= MinimumPoolSize);
            Assert.IsTrue(_regionPool.Length >= MinimumPoolSize);


            var voxelWorld = new VoxelWorld(this);
            _endSimBarrier.AddJobHandleForProducer(Dependency);
            return voxelWorld;
        }

        public VoxelWorldReadOnly GetVoxelWorldReadOnly()
        {
            return new VoxelWorldReadOnly(this);
        }

        struct MappedRegion : ISystemStateComponentData
        {
            public int2 Index;
        }

        struct MappedChunk : ISystemStateComponentData
        {
            public int3 Index;
        }

        public Entity GetRegionPrefab() => _prefabLoader.RegionPrefab;
        public Entity GetVoxelChunkPrefab() => _prefabLoader.VoxelChunkPrefab;

    }
}