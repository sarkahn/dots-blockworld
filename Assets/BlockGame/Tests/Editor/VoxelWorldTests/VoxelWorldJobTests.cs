using NUnit.Framework;
using Sark.EcsTesting;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Sark.BlockGame.Tests
{
    [TestFixture]
    public class VoxelWorldJobTests : WorldTestBase
    {
        VoxelWorldSystem System;
        VoxelWorld VoxelWorld => System.GetVoxelWorld();

        [SetUp]
        public void Setup()
        {
            System = AddSystem<VoxelWorldSystem>();
        }

        struct WorldHavingJob : IJob
        {
            public VoxelWorld world;

            public void Execute()
            {
            }
        }

        struct SetBlockJob : IJob
        {
            public VoxelWorld World;
            public int3 Position;
            public ushort Block;

            public void Execute()
            {
                World.SetBlock(Position, Block);
            }
        }

        struct ReadBlockJob : IJob
        {
            [ReadOnly]
            public VoxelWorldReadOnly World;
            public int3 ReadPosition;
            public NativeArray<int> Output;

            public void Execute()
            {
                Output[0] = World.GetBlock(ReadPosition);
            }
        }

        [Test]
        public void BuildingVoxelWorldStructDoesntCauseExceptions()
        {
            Assert.DoesNotThrow(() => { var world = System.GetVoxelWorld(); });
        }

        [Test]
        public void VoxelWorldCanBeUsedInJobs()
        {
            JobHandle job1 = default;
            JobHandle job2 = default;
            var world = VoxelWorld;
            Assert.DoesNotThrow(() =>
            {
                job1 = new WorldHavingJob { world = world }.Schedule();
            });

            Assert.DoesNotThrow(() =>
            {
                job2 = new WorldHavingJob { world = world }.Schedule(job1);
            });

            job2.Complete();
        }


        [Test]
        public void SetBlockInsideJob()
        {
            var world = VoxelWorld;

            new SetBlockJob
            {
                World = world,
                Block = 5,
                Position = new int3(0, 5, 10)
            }.Schedule().Complete();

            var block = world.GetBlock(0, 5, 10);

            Assert.AreEqual(5, block);
        }

        [Test]
        public void MultipleJobsDependencyTest()
        {
            var world = VoxelWorld;

            var job1 = new SetBlockJob
            {
                World = world,
                Block = 10,
                Position = new int3(0, 5, 6)
            }.Schedule();

            var job2 = new SetBlockJob
            {
                World = world,
                Block = 15,
                Position = new int3(35, 8, 7)
            }.Schedule(job1);

            var job3 = new SetBlockJob
            {
                World = world,
                Block = 7,
                Position = new int3(0, 5, 6)
            }.Schedule(job2);

            job3.Complete();

            var block1 = world.GetBlock(0, 5, 6);
            var block2 = world.GetBlock(35, 8, 7);

            Assert.AreEqual(7, block1);
            Assert.AreEqual(15, block2);
        }

        [Test]
        public void DependencyThroughVoxelWorldSystemTest()
        {
            var world = VoxelWorld;

            var outputDeps = System.GetOutputDependency();
            var job1 = new SetBlockJob
            {
                World = world,
                Block = 12,
                Position = new int3(1, 1, 1)
            }.Schedule(outputDeps);
            System.AddInputDependency(job1);

            outputDeps = System.GetOutputDependency();
            var job2 = new SetBlockJob
            {
                World = world,
                Block = 99,
                Position = new int3(12, 5, 33)
            }.Schedule(outputDeps);
            System.AddInputDependency(job2);

            outputDeps = System.GetOutputDependency();
            var job3 = new SetBlockJob
            {
                World = world,
                Block = 103,
                Position = new int3(0, 0, 6)
            }.Schedule(outputDeps);
            System.AddInputDependency(job3);

            World.Update();

            world = VoxelWorld;

            var block1 = world.GetBlock(1, 1, 1);
            var block2 = world.GetBlock(12, 5, 33);
            var block3 = world.GetBlock(0, 0, 6);

            Assert.AreEqual(12, block1);
            Assert.AreEqual(99, block2);
            Assert.AreEqual(103, block3);
        }

        [Test]
        public void ReadOnlyWorldTest()
        {
            var world = VoxelWorld;

            world.SetBlock(0, 0, 0, 15);
            world.SetBlock(0, 3, 10, 30);

            var readOnlyWorld = System.GetVoxelWorldReadOnly();

            var output1 = new NativeArray<int>(1, Allocator.TempJob);
            var output2 = new NativeArray<int>(1, Allocator.TempJob);

            var readJob1 = new ReadBlockJob
            {
                World = readOnlyWorld,
                ReadPosition = new int3(0, 0, 0),
                Output = output1
            }.Schedule();

            var readJob2 = new ReadBlockJob
            {
                World = readOnlyWorld,
                ReadPosition = new int3(0, 3, 10),
                Output = output2
            }.Schedule();

            var combined = JobHandle.CombineDependencies(readJob1, readJob2);

            combined.Complete();

            Assert.AreEqual(15, output1[0]);
            Assert.AreEqual(30, output2[0]);

            output1.Dispose();
            output2.Dispose();
        }

        struct SetBlockJob2 : IJob
        {
            public VoxelWorld World2;
            public int3 Position;
            public ushort Block;

            public void Execute()
            {
                World2.SetBlock(Position, Block);
            }
        }

        struct ReadBlockJob2 : IJob
        {
            [ReadOnly]
            public VoxelWorldReadOnly Reader;
            public int3 ReadPosition;
            public NativeArray<ushort> Output;

            public void Execute()
            {
                Output[0] = Reader.GetBlock(ReadPosition);
            }
        }

        [Test]
        public void WriteAndMultiReadTest()
        {
            var world2 = System.GetVoxelWorld();

            var job1 = new SetBlockJob2
            {
                World2 = world2,
                Block = 9,
                Position = new int3(30, 30, 5)
            }.Schedule();

            var job2 = new SetBlockJob2
            {
                World2 = world2,
                Block = 50,
                Position = new int3(5, 5, 5)
            }.Schedule(job1);

            job2.Complete();

            Assert.AreEqual(9, world2.GetBlock(30, 30, 5));
            Assert.AreEqual(50, world2.GetBlock(5, 5, 5));

            var output1 = new NativeArray<ushort>(1, Allocator.TempJob);
            var output2 = new NativeArray<ushort>(1, Allocator.TempJob);

            var reader = System.GetVoxelWorldReadOnly();

            var readJob1 = new ReadBlockJob2
            {
                Output = output1,
                ReadPosition = new int3(30, 30, 5),
                Reader = reader
            }.Schedule();

            var readJob2 = new ReadBlockJob2
            {
                Output = output2,
                ReadPosition = new int3(5, 5, 5),
                Reader = reader
            }.Schedule();

            var parallelJobs = JobHandle.CombineDependencies(readJob1, readJob2);

            parallelJobs.Complete();

            Assert.AreEqual(9, output1[0]);
            Assert.AreEqual(50, output2[0]);

            output1.Dispose();
            output2.Dispose();
        }


    }
}