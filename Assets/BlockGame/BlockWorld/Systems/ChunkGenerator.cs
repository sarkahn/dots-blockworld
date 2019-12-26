using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Grid2D = GridMath.Grid2D;
using Grid3D = GridMath.Grid3D;

namespace BlockWorld
{
    [DisableAutoCreation]
    public class ChunkGenerator : JobComponentSystem
    {
        BeginSimulationEntityCommandBufferSystem bufferSystem;

        EntityQuery eq;

        NativeStream stream;
        

        EntityQuery uninitializedChunks;

        NativeList<JobHandle> fillHeightMapJobs;

        NativeList<Entity> changedChunkEntities;

        public readonly int3 cellSize = new int3(3, 3, 3);
        
        
        struct PopulateHeight : IJobParallelFor
        {
            [ReadOnly]
            public Entity bufferEntity;
            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public BufferFromEntity<ChunkHeight> bfe;
            [ReadOnly]
            public int3 chunkIndex;
            public int3 cellSize;

            public void Execute(int index)
            {
                // TODO : Need to account for chunkindex so w're not passing the same
                // values on different chunks over and over to noise
                int3 xyz = Grid3D.CellPosFromArrayIndex(index, cellSize);
                float v = noise.snoise(xyz.xz);
                v = (v / 2f) + .5f;

                int height = (int)math.lerp(0, cellSize.y, v);
                var buffer = bfe[bufferEntity];
                Debug.Log("Writing to index " + index);
                buffer[index] = height;
                //var heightMap = bfe[bufferEntity].Reinterpret<int>().AsNativeArray();
                //heightMap[index] = height;
                
            }
        }



        Entity MakeChunkEntity(int x, int y, int z) => MakeChunkEntity(new int3(x, y, z));
        Entity MakeChunkEntity(int3 index)
        {
            var e = EntityManager.CreateEntity();
            EntityManager.AddSharedComponentData<BlockChunkIndex>(e, index);
            EntityManager.SetName(e, $"Chunk {index.x}, {index.y}, {index.z}");
            return e;
        }

        protected override void OnCreate()
        {
            bufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
            fillHeightMapJobs = new NativeList<JobHandle>(10, Allocator.Persistent);
            //chunkEntities = new NativeQueue<Entity>(Allocator.Persistent);

            // TEST
            //eq = GetEntityQuery(typeof(BlockBuffer));

            changedChunkEntities = new NativeList<Entity>(100, Allocator.Persistent);

            MakeChunkEntity(0, 1, 1);
            MakeChunkEntity(0, 2, 1);

            uninitializedChunks = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(BlockChunkIndex)},
                None = new ComponentType[] {typeof(ChunkHeight)}
            });
        }

        protected override void OnDestroy()
        {
            fillHeightMapJobs.Dispose();
            //chunkEntities.Dispose();

            changedChunkEntities.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //var entities = uninitializedChunks.ToEntityArray(Allocator.TempJob);
            //var commandBuffer = bufferSystem.CreateCommandBuffer();
            
            
            //for( int i = 0; i < entities.Length; ++i )
            //{
            //    var buffer = EntityManager.AddBuffer<ChunkHeight>(entities[i]);
            //    buffer.ResizeUninitialized(Grid3D.CellVolumeXZ(cellSize));
            //}
            
            //fillHeightMapJobs.Clear();
            //for(int i = 0; i < entities.Length; ++i )
            //{
            //    Entity e = entities[i];
            //    Debug.Log($"Scheduling job for entity {EntityManager.GetName(e)}");
            //    fillHeightMapJobs.Add(new PopulateHeight
            //    {
            //        bufferEntity = e,
            //        bfe = GetBufferFromEntity<ChunkHeight>(false),
            //        chunkIndex = EntityManager.GetSharedComponentData<ChunkIndex>(e),
            //        cellSize = cellSize
            //    }.Schedule(Grid3D.CellVolumeXZ(cellSize), 32, inputDeps));
            //}

            //var fillJobs = JobHandle.CombineDependencies(fillHeightMapJobs);

            //entities.Dispose(fillJobs);

            //var buffer = bufferSystem.CreateCommandBuffer();

            //Job.WithReadOnly(uninitializedChunks).WithCode(() =>
            //{
            //    buffer.AddComponent(uninitializedChunks, typeof(ChunkHeight));
            //}).Schedule(inputDeps);



            //int heightMapVolume = ChunkMath.chunkSizeX * ChunkMath.chunkSizeZ;

            //Entities.WithNone<ChunkHeight>().ForEach((Entity e) =>
            //{
            //}).Schedule(inputDeps);


            //Entities.WithNone<ChunkHeight>().WithStructuralChanges().ForEach(
            //    (Entity e, in ChunkIndex chunkIndex) =>
            //    {
            //        Debug.Log("GENERATING BUFFER");
            //        var buffer = EntityManager.AddBuffer<ChunkHeight>(e);
            //        buffer.ResizeUninitialized(heightMapVolume);
            //        Debug.Log("BUFFER GENERATED");
            //        var fillJob = new PopulateHeight
            //        {
            //            chunkIndex = chunkIndex,
            //            bufferEntity = e,
            //            bfe = GetBufferFromEntity<ChunkHeight>(false),
            //        }.Schedule(buffer.Length, 32, inputDeps);
            //        fillHeightMapJobs.Add(fillJob);
            //    }).Run();
            //inputDeps = JobHandle.CombineDependencies(fillHeightMapJobs);

            //for( int i = 0; i < chunkCount; ++i )
            //{
            //    NativeArray<int> heightMapArray = new NativeArray<int>(heightMapVolume, Allocator.TempJob);

            //    var fillJob = new PopulateHeight
            //    {
            //        heightMap = heightMapArray,
            //        chunkIndex = 
            //    }.Schedule(heightMapVolume, 64, inputDeps);

            //    fillHeightMapJobs = JobHandle.CombineDependencies(fillHeightMapJobs, fillJob);
            //}

            //inputDeps = JobHandle.CombineDependencies(inputDeps, fillHeightMapJobs)


            //inputDeps = Entities.WithAll<ChunkIndex>().WithNone<ChunkHeight>().ForEach((Entity e) =>
            //{

            //}).Schedule(inputDeps);



            //var buffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

            //// Generate height map
            //inputDeps = Entities.WithAll<GenerateChunkTest>().ForEach(
            //    (Entity e, int entityInQueryIndex, ref DynamicBuffer<ChunkHeight> chunkHeight) =>
            //    {
            //        buffer.RemoveComponent<GenerateChunkTest>(entityInQueryIndex, e);

            //        for( int x = 0; x < ChunkMath.chunkSizeX; ++x )
            //        {
            //            for( int z = 0; z < ChunkMath.chunkSizeZ; ++z )
            //            {
            //                float v = noise.snoise(new float2(x, z));
            //                v = (v / 2f) + .5f;

            //                int height = (int)math.lerp(0, ChunkMath.chunkSizeY, v);
            //                int i = z * ChunkMath.chunkSizeX + x;
            //                chunkHeight[i] = height;
            //            }
            //        }
            //    }).Schedule(inputDeps);


            //while(chunkEntities.Count > 0 )
            //{
            //    EntityQuery eq;
            //}
            //int heightMapCells = ChunkMath.chunkSizeX * ChunkMath.chunkSizeZ;
            //inputDeps = new PopulateHeight
            //{
            //}.Schedule(heightMapCells, 64, inputDeps);

            //  bufferSystem.AddJobHandleForProducer(inputDeps);



            //inputDeps = new ReadArray
            //{
            //    entities = blocks
            //}.Schedule(blocks.Length, 10, inputDeps);


            return inputDeps;
        }

    }
}
