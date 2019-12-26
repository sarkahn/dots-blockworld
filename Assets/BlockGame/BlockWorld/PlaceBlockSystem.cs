using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace BlockWorld
{
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PlaceBlockSystem : JobComponentSystem
    {
        BeginInitializationEntityCommandBufferSystem _bufferSystem;

        EntityQuery _placeBlocksQuery;
        EntityQuery _bufferQuery;

        protected override void OnCreate()
        {
            _bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

            _placeBlocksQuery = GetEntityQuery(
                ComponentType.ReadOnly<PlaceBlock>(),
                ComponentType.ReadOnly<BlockChunkIndex>()
                );

            _bufferQuery = GetEntityQuery(
                ComponentType.ReadWrite<BlockBuffer>(),
                ComponentType.ReadOnly<BlockChunkIndex>()
                );
        }

        struct ProcessBlockDeltas : IJobChunk
        {
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Block> blockFromEntity;

            [ReadOnly]
            public NativeArray<PlaceBlock> blockDeltas;

            public ArchetypeChunkBufferType<BlockBuffer> blockBufferType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                // Assumes only one block buffer per chunk
                var buffer = chunk.GetBufferAccessor(blockBufferType)[0].Reinterpret<Entity>().AsNativeArray();
                for( int i = 0; i < blockDeltas.Length; ++i )
                {
                    var delta = blockDeltas[i];
                    int blockIndex = GridMath.Grid3D.ArrayIndexFromWorldPos(delta.pos, Constants.BlockChunks.Size);
                    var blockEntity = buffer[blockIndex];
                    var block = blockFromEntity[blockEntity];
                    block.type = delta.blockType;
                    blockFromEntity[blockEntity] = block;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = _bufferSystem.CreateCommandBuffer();
            var concurrentCommandBuffer = commandBuffer.ToConcurrent();

            NativeList<int3> indices = new NativeList<int3>(Allocator.TempJob);

            Entities
                .WithStructuralChanges()
                .ForEach(
                (Entity e, in PlaceBlock pb) =>
                {
                    int3 chunkIndex = GridMath.Grid3D.CellIndexFromWorldPos(pb.pos, Constants.BlockChunks.Size);
                    if (!indices.Contains(chunkIndex))
                        indices.Add(chunkIndex);
                    EntityManager.AddSharedComponentData<BlockChunkIndex>(e, chunkIndex);
                }).Run();

            var blockFromEntity = GetComponentDataFromEntity<Block>(false);
            for( int i = 0; i < indices.Length; ++i )
            {
                var chunkIndex = indices[i];
                _placeBlocksQuery.SetSharedComponentFilter<BlockChunkIndex>(chunkIndex);
                var deltas = _placeBlocksQuery.ToComponentDataArray<PlaceBlock>(Allocator.TempJob);

                _bufferQuery.SetSharedComponentFilter<BlockChunkIndex>(chunkIndex);

                inputDeps = new ProcessBlockDeltas
                {
                    blockBufferType = GetArchetypeChunkBufferType<BlockBuffer>(false),
                    blockDeltas = deltas,
                    blockFromEntity = GetComponentDataFromEntity<Block>(false)
                }.Schedule(_bufferQuery, inputDeps);

                //inputDeps = Entities
                //    .WithSharedComponentFilter<ChunkIndex>(chunkIndex)
                //    .WithNativeDisableParallelForRestriction(blockFromEntity)
                //    .WithReadOnly(deltas)
                //    .ForEach((ref DynamicBuffer<BlockBuffer> blockBuffer) =>
                //    {
                //        for( int deltaIndex = 0; deltaIndex < deltas.Length; ++deltaIndex )
                //        {
                //            var pb = deltas[deltaIndex];
                //            int blockIndex = GridMath.Grid3D.ArrayIndexFromWorldPos(pb.pos, Constants.ChunkSize);
                //            var blockEntity = blockBuffer[blockIndex];
                //            var block = blockFromEntity[blockEntity];
                //            //block.type = pb.blockType;
                //            //blockFromEntity[blockEntity] = block;
                //            //blockBuffer[blockIndex] = blockEntity;
                //        }
                //    }).Schedule(inputDeps);

                deltas.Dispose(inputDeps);
            }

            indices.Dispose(inputDeps);

            commandBuffer.DestroyEntity(_placeBlocksQuery);

            _bufferSystem.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
        
    }
}