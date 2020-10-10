using Unity.Entities;
using BlockGame.Chunks;
using Unity.Collections;
using BlockGame.Regions;
using Unity.Mathematics;
using Unity.Jobs;
using GridUtil;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
using Unity.Assertions;
using Unity.Entities.Hybrid;
using BlockGame.CollectionExtensions;

namespace BlockGame.VoxelWorld
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

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        void SetupPools()
        {
            if (!_regionBuilder.IsRunning && _regionPool.Length < MinimumPoolSize)
                _regionBuilder.ScheduleCreateRegionsJob(RegionPoolRefillAmount);
            else if (_regionBuilder.IsRunning && _regionBuilder.IsCompleted)
                CompleteRegionBuilderJob();

            if (!_chunkBuilder.IsRunning && _chunkPool.Length < MinimumPoolSize)
                _chunkBuilder.ScheduleCreateChunksJob(ChunkPoolRefillAmount);
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

    }
}