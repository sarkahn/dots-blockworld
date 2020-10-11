

using NUnit.Framework;
using NUnit.Framework.Constraints;
using Sark.EcsTesting;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Sark.BlockGame.Tests
{
    [TestFixture]
    public class EntityStagingTest : WorldTestBase
    {
        [Test]
        public void TestBuildRegionsJob()
        {
            var builder = new EntityStagingHelper("Regions");
            var q = GetEntityQuery(typeof(Region), typeof(Disabled));

            var regionArchetype = EntityManager.CreateArchetype(
                typeof(Region),
                typeof(LinkedEntityGroup),
                typeof(Disabled)
                );
            var arr = new NativeArray<Entity>(100, Allocator.TempJob);
            EntityManager.CreateEntity(regionArchetype, arr);

            builder.ScheduleCreateRegionsJob(100);
            var entities = builder.Complete(World);

            EntityManager.CreateEntity(regionArchetype, arr);

            Assert.AreEqual(100, entities.Length);
            Assert.AreEqual(300, q.CalculateEntityCount());
            var buffer = EntityManager.GetBuffer<LinkedEntityGroup>(entities[0]);

            Assert.AreEqual(entities[0], buffer[0].Value);

            entities.Dispose();
            arr.Dispose();
        }

        [Test]
        public void TestBuildRegionsAndChunksSimultaneously()
        {
            var regionBuilder = new EntityStagingHelper("Regions");
            var chunksBuilder = new EntityStagingHelper("Chunks");

            var regionsQ = GetEntityQuery(typeof(Region), typeof(Disabled));
            var chunksQ = GetEntityQuery(typeof(VoxelChunk), typeof(Disabled));

            var prefabs = new WorldEntityPrefabLoader(EntityManager);

            var regionJob = regionBuilder.ScheduleCreateRegionsJob(
                100).StagingJob;
            var chunkJob = chunksBuilder.ScheduleCreateChunksJob(
                100).StagingJob;

            var parallelJob = JobHandle.CombineDependencies(regionJob, chunkJob);

            Assert.DoesNotThrow(() =>
            {
                parallelJob.Complete();
            // Must call complete on builders to end transactions and move staged entities
            regionBuilder.Complete(World).Dispose();
                chunksBuilder.Complete(World).Dispose();
            });

            Assert.AreEqual(100, chunksQ.CalculateEntityCount());
            Assert.AreEqual(100, regionsQ.CalculateEntityCount());
        }
    }
}