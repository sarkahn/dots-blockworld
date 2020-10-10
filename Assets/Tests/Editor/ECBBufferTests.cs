
using BlockGame.Chunks;
using NUnit.Framework;
using Sark.EcsTesting;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[TestFixture]
public class ECBBufferTests : WorldTestBase
{
    [DisableAutoCreation]
    class ECBBufferTestSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        protected override void OnCreate()
        {
            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endSimBarrier.CreateCommandBuffer().AsParallelWriter();
            var archetype = EntityManager.CreateArchetype(typeof(VoxelChunkBlocks));
            var bfe = GetBufferFromEntity<VoxelChunkBlocks>(false);

            Entities.ForEach((int entityInQueryIndex, Entity e, Translation t) =>
            {
                var newEntity = ecb.CreateEntity(entityInQueryIndex, archetype);

                var buffer = ecb.SetBuffer<VoxelChunkBlocks>(entityInQueryIndex, newEntity);
                buffer.ResizeUninitialized(100);
                for (int i = 0; i < 100; ++i)
                    buffer[i] = 0;
            }).Schedule();

            _endSimBarrier.AddJobHandleForProducer(Dependency);

            Enabled = false;
        }
    }

    [Test]
    public void CanGetBufferAfterImmediatelyCreatingIt()
    {
        Entity dummy = EntityManager.CreateEntity(typeof(Translation));
        var ecbTest = World.GetOrCreateSystem<ECBBufferTestSystem>();

        ecbTest.Update();
        World.Update();

        var e = CreateEntityQuery(typeof(VoxelChunkBlocks)).GetSingletonEntity();

        Assert.AreNotEqual(Entity.Null, e);

        var buffer = EntityManager.GetBuffer<VoxelChunkBlocks>(e);

        Assert.AreEqual(100, buffer.Length);
        Assert.AreEqual(0, buffer[0].BlockType);
    }
}
