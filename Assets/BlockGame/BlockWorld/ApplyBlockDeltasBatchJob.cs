using UnityEngine;
using System.Collections;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

namespace BlockWorld
{
    public struct ApplyRegionBlockDeltasJob : IJob
    {
        public EntityCommandBuffer CommandBuffer;

        [ReadOnly]
        public NativeArray<BlockDelta> Deltas;
        
        public NativeHashMap<int3, Entity> ChunkMap;
        
        [WriteOnly]
        public BufferFromEntity<BlockBuffer> BlockBufferFromEntity;

        public void Execute()
        {
            for( int i = 0; i < Deltas.Length; ++i )
            {
                var delta = Deltas[i];
                int3 chunkIndex = GridMath.Grid3D.CellIndexFromWorldPos(delta.WorldPos, Constants.BlockChunks.Size);

                DynamicBuffer<BlockBuffer> buffer;
                Entity chunkEntity;
                if (!ChunkMap.TryGetValue(chunkIndex, out chunkEntity))
                {
                    ChunkMap[chunkIndex] = chunkEntity = CommandBuffer.CreateEntity();
                    buffer = CommandBuffer.AddBuffer<BlockBuffer>(chunkEntity);
                    buffer.ResizeUninitialized(Constants.BlockChunks.Volume);
                }
                else
                    buffer = BlockBufferFromEntity[chunkEntity];

                int blockIndex = GridMath.Grid3D.ArrayIndexFromWorldPos(delta.WorldPos, Constants.BlockChunks.Size);
                buffer[blockIndex] = delta.BlockType;
            }
        }
    } 
}
