using BlockGame.Chunks;
using BlockGame.Regions;
using BlockGame.VoxelWorld;
using NUnit.Framework;
using Sark.EcsTesting;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.VoxelWorldTests
{
    [TestFixture]
    public class VoxelWorldTests : WorldTestBase
    {
        VoxelWorldSystem System;
        VoxelWorldSystem.VoxelWorld VoxelWorld => System.GetVoxelWorld();

        [SetUp]
        public void Setup()
        {
            System = AddSystem<VoxelWorldSystem>();
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(5, 6)]
        [TestCase(10, 45)]
        [TestCase(-10, 85)]
        public void GetOrCreateRegionTest(int x, int y)
        {
            var targetIndex = new int2(x, y);
            var region = VoxelWorld.GetOrCreateRegionFromIndex(targetIndex);

            Assert.IsTrue(EntityManager.HasComponent<Region>(region));
            Assert.IsTrue(EntityManager.HasComponent<VoxelChunkStack>(region));
            Assert.IsTrue(EntityManager.HasComponent<LinkedEntityGroup>(region));

            var linkedGroup = EntityManager.GetBuffer<LinkedEntityGroup>(region);

            int2 regionIndex = EntityManager.GetComponentData<Region>(region).Index;
            Assert.AreEqual(targetIndex, regionIndex);
            Assert.AreEqual(region, linkedGroup[0].Value);

            Assert.IsTrue(EntityManager.HasComponent<Disabled>(region));

            World.Update();

            Assert.IsFalse(EntityManager.HasComponent<Disabled>(region));
        }

        [Test]
        public void GetOrCreateVoxelChunkTest()
        {
            var chunk1Entity = VoxelWorld.GetOrCreateVoxelChunkFromIndex(0, 0, 0);
            var chunk2Entity = VoxelWorld.GetOrCreateVoxelChunkFromIndex(1, 0, 0);

            Assert.AreNotEqual(chunk1Entity, chunk2Entity);

            var chunk1 = EntityManager.GetComponentData<VoxelChunk>(chunk1Entity);
            var chunk2 = EntityManager.GetComponentData<VoxelChunk>(chunk2Entity);

            var region1 = chunk1.Region;
            var region2 = chunk2.Region;

            Assert.AreNotEqual(region1, region2);

            var region1FromWorld = VoxelWorld.GetOrCreateRegionFromIndex(0, 0);
            var region2FromWorld = VoxelWorld.GetOrCreateRegionFromIndex(1, 0);

            Assert.AreEqual(region1, region1FromWorld);
            Assert.AreEqual(region2, region2FromWorld);

            var blocks1 = EntityManager.GetBuffer<VoxelChunkBlocks>(chunk1Entity);
            var blocks2 = EntityManager.GetBuffer<VoxelChunkBlocks>(chunk2Entity);

            var blocks1FromWorld = VoxelWorld.GetOrCreateBlocksArrayFromIndex(0, 0, 0);
            var blocks2FromWorld = VoxelWorld.GetOrCreateBlocksArrayFromIndex(1, 0, 0);

            blocks1[0] = 5;
            Assert.AreEqual(5, blocks1FromWorld[0]);

            blocks2[5] = 10;
            Assert.AreEqual(10, blocks2FromWorld[5]);
        }

        [Test]
        public void SetBlockTest()
        {
            var world = VoxelWorld;
            world.SetBlock(5, 3, 15, 5);

            var block = world.GetBlock(5, 3, 15);

            Assert.AreEqual(5, block);
        }

        [Test]
        public void AdjacentChunkBlocksTest()
        {
            var world = VoxelWorld;

            world.SetBlock(0, 0, 0, 5);
            world.SetBlock(16, 0, 0, 5);

            bool found = world.TryGetRegionFromIndex(1, 0, out var region);

            Assert.IsTrue(found);
            Assert.AreNotEqual(Entity.Null, region);

            // Note the struct must be recreated if a new chunk has been generated
            var adj = world.GetAdjacentChunkBlocks(0, 0, 0);

            Assert.IsTrue(adj.East.IsCreated);

            world.SetBlock(0, 16, 0, 1);

            adj = world.GetAdjacentChunkBlocks(0, 0, 0);

            Assert.IsTrue(adj.Up.IsCreated);
        }
    }

}