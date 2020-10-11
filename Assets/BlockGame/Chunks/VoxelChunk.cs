using GridUtil;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.Chunks
{
    /// <summary>
    /// Represents a 16x16x16 section of blocks. Each value is the block type at a single position.
    /// </summary>
    [InternalBufferCapacity(Grid3D.CellVolume)]
    public struct VoxelChunkBlocks : IBufferElementData
    {
        public ushort BlockType;
        public static implicit operator ushort(VoxelChunkBlocks b) => b.BlockType;
        public static implicit operator VoxelChunkBlocks(ushort v) => new VoxelChunkBlocks { BlockType = v };
    } 

    public struct VoxelChunk : IComponentData
    {
        public int3 Index;
        public Entity Region;
    }

    public struct VoxelChunkBuilder
    {
        EntityArchetype _archetype;

        public VoxelChunkBuilder(SystemBase system)
        {
            _archetype = system.EntityManager.CreateArchetype(
                typeof(VoxelChunk),
                typeof(VoxelChunkBlocks),
                typeof(ChunkMeshVerts),
                typeof(ChunkMeshIndices),
                typeof(ChunkMeshUVs)
                );
        }

        public Entity CreateVoxelChunk(EntityCommandBuffer ecb, int3 chunkIndex, Entity region)
        {
            var chunk = ecb.CreateEntity(_archetype);
            var blocks = ecb.SetBuffer<VoxelChunkBlocks>(chunk);
            blocks.ResizeUninitialized(Grid3D.CellVolume);
            unsafe
            {
                UnsafeUtility.MemClear(blocks.GetUnsafePtr(), blocks.Length);
            }

            ecb.SetComponent(chunk, new VoxelChunk
            {
                Index = chunkIndex,
                Region = region
            });

            ecb.AppendToBuffer<LinkedEntityGroup>(region, chunk);

            return chunk;
        }

        public Entity CreateVoxelChunk(EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, int3 chunkIndex, Entity region)
        {
            var chunk = ecb.CreateEntity(entityInQueryIndex, _archetype);
            var blocks = ecb.SetBuffer<VoxelChunkBlocks>(entityInQueryIndex, chunk);
            blocks.ResizeUninitialized(Grid3D.CellVolume);
            unsafe
            {
                UnsafeUtility.MemClear(blocks.GetUnsafePtr(), blocks.Length);
            }

            ecb.SetComponent(entityInQueryIndex, chunk, new VoxelChunk
            {
                Index = chunkIndex,
                Region = region
            });

            ecb.AppendToBuffer<LinkedEntityGroup>(entityInQueryIndex, region, chunk);

            return chunk;
        }

        public void CreateVoxelChunks(ExclusiveEntityTransaction tr, int amount, int3 chunkIndex, Entity region)
        {
            var arr = new NativeArray<Entity>(amount, Allocator.Temp);
            tr.CreateEntity(_archetype, arr);
            for (int i = 0; i < arr.Length; ++i)
            {
                var blocks = tr.GetBuffer<VoxelChunkBlocks>(arr[i]);
                blocks.ResizeUninitialized(Grid3D.CellVolume);
                unsafe
                {
                    UnsafeUtility.MemClear(blocks.GetUnsafePtr(), blocks.Length);
                }
            }
        }
    }
}
