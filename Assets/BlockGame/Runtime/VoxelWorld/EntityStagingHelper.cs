using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Assertions;

namespace Sark.BlockGame
{
    /// <summary>
    /// Stores a World for building entities inside a job.
    /// Due to how <see cref="ExclusiveEntityTransaction"/> works one builder
    /// can only handle a single job at a time, but multiple builders can operate
    /// in parallel.
    /// </summary>
    public class EntityStagingHelper
    {
        public World StagingWorld { get; private set; }
        public EntityManager StagingManager => StagingWorld.EntityManager;

        public bool IsRunning { get; private set; }
        public bool IsCompleted => StagingJob.IsCompleted;

        public JobHandle StagingJob { get; private set; }

        WorldEntityPrefabLoader _prefabLoader;

        public EntityStagingHelper(string name)
        {
            StagingWorld = new World($"{name} Staging World");
            _prefabLoader = new WorldEntityPrefabLoader(StagingManager);
        }

        public NativeArray<Entity> Complete(World targetWorld)
        {
            IsRunning = false;
            StagingJob.Complete();
            StagingManager.EndExclusiveEntityTransaction();
            targetWorld.EntityManager.MoveEntitiesFrom(out var array, StagingManager);
            return array;
        }

        public EntityStagingHelper ScheduleCreateRegionsJob(
            int amount,
            JobHandle inputDeps = default)
        {
            return ScheduleCreateEntitiesJob<RegionBuilderJob>(
                amount, 
                _prefabLoader.RegionPrefab, 
                inputDeps);
        }

        public EntityStagingHelper ScheduleCreateChunksJob(
            int amount,
            JobHandle inputDeps = default)
        {
            return ScheduleCreateEntitiesJob<ChunkBuilderJob>(
                amount, 
                _prefabLoader.VoxelChunkPrefab, 
                inputDeps);
        }

        EntityStagingHelper ScheduleCreateEntitiesJob<T>(
            int amount,
            Entity prefab,
            JobHandle inputDeps = default) where T : unmanaged, IEntityBuilderJob
        {
            Assert.IsFalse(IsRunning);

            IsRunning = true;
            var tr = StagingManager.BeginExclusiveEntityTransaction();
            StagingJob = new T
            {
                Amount = amount,
                Transaction = tr,
                Prefab = prefab
            }.Schedule(inputDeps);

            return this;
        }
    }

    public struct RegionBuilderJob : IEntityBuilderJob
    {
        public ExclusiveEntityTransaction Transaction { get; set; }
        public int Amount { get; set; }

        public Entity Prefab { get; set; }

        public void Execute()
        {
            var arr = new NativeArray<Entity>(Amount, Allocator.Temp);
            Transaction.Instantiate(Prefab, arr);
            for (int i = 0; i < arr.Length; ++i)
            {
                Transaction.AddComponent(arr[i], typeof(Disabled));
            }

            Transaction.DestroyEntity(Prefab);
        }
    }

    public struct ChunkBuilderJob : IEntityBuilderJob
    {
        public ExclusiveEntityTransaction Transaction { get; set; }
        public int Amount { get; set; }
        public VoxelChunkBuilder Builder { get; set; }
        public Entity Prefab { get; set; }

        public void Execute()
        {
            var arr = new NativeArray<Entity>(Amount, Allocator.Temp);
            Transaction.Instantiate(Prefab, arr);
            for (int i = 0; i < arr.Length; ++i)
            {
                Transaction.AddComponent(arr[i], typeof(Disabled));
            }

            Transaction.DestroyEntity(Prefab);
        }
    }
    public interface IEntityBuilderJob : IJob
    {
        ExclusiveEntityTransaction Transaction { get; set; }
        int Amount { get; set; }
        Entity Prefab { get; set; }
    }
}