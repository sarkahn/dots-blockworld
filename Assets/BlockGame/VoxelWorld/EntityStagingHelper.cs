using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using System.Threading.Tasks;
using BlockGame.Regions;
using BlockGame.Chunks;
using GridUtil;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Assertions;

namespace BlockGame.VoxelWorldNS
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

        public EntityStagingHelper(string name)
        {
            StagingWorld = new World($"{name} Staging World");
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
            JobHandle inputDeps = default) =>
            ScheduleCreateEntitiesJob<RegionBuilderJob>(amount, inputDeps);

        public EntityStagingHelper ScheduleCreateChunksJob(
            int amount,
            JobHandle inputDeps = default) =>
            ScheduleCreateEntitiesJob<ChunkBuilderJob>(amount, inputDeps);

        EntityStagingHelper ScheduleCreateEntitiesJob<T>(
            int amount,
            JobHandle inputDeps = default) where T : unmanaged, IEntityBuilderJob
        {
            Assert.IsFalse(IsRunning);

            IsRunning = true;
            var tr = StagingManager.BeginExclusiveEntityTransaction();
            StagingJob = new T
            {
                Amount = amount,
                Transaction = tr
            }.Schedule(inputDeps);

            return this;
        }
    }

    public struct RegionBuilderJob : IEntityBuilderJob
    {
        public ExclusiveEntityTransaction Transaction { get; set; }
        public int Amount { get; set; }

        public void Execute()
        {
            var arch = Transaction.CreateArchetype(
                typeof(Region),
                typeof(VoxelChunkStack),
                typeof(LinkedEntityGroup),
                typeof(Disabled));
            var arr = new NativeArray<Entity>(Amount, Allocator.Temp);
            Transaction.CreateEntity(arch, arr);
            for (int i = 0; i < arr.Length; ++i)
            {
                var buffer = Transaction.GetBuffer<LinkedEntityGroup>(arr[i]);
                buffer.Add(arr[i]);
            }
        }
    }

    public struct ChunkBuilderJob : IEntityBuilderJob
    {
        public ExclusiveEntityTransaction Transaction { get; set; }
        public int Amount { get; set; }

        public void Execute()
        {
            var arch = Transaction.CreateArchetype(
                typeof(VoxelChunk),
                typeof(VoxelChunkBlocks),
                typeof(Disabled)
                );
            var arr = new NativeArray<Entity>(Amount, Allocator.Temp);
            Transaction.CreateEntity(arch, arr);
            for (int i = 0; i < arr.Length; ++i)
            {
                var blocks = Transaction.GetBuffer<VoxelChunkBlocks>(arr[i]);
                blocks.ResizeUninitialized(Grid3D.CellVolume);
                unsafe
                {
                    UnsafeUtility.MemClear(blocks.GetUnsafePtr(), blocks.Length);
                }
            }
        }
    }
    public interface IEntityBuilderJob : IJob
    {
        ExclusiveEntityTransaction Transaction { get; set; }
        int Amount { get; set; }
    }
}