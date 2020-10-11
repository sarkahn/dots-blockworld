using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    public class VoxelChunkAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int3 chunkIndex = Grid3D.WorldToCell(transform.position);
            dstManager.AddComponentData<VoxelChunk>(entity, new VoxelChunk { Index = chunkIndex });

            var blocks = dstManager.AddBuffer<VoxelChunkBlocks>(entity);
            blocks.ResizeUninitialized(Grid3D.CellVolume);
            unsafe
            {
                UnsafeUtility.MemClear(blocks.GetUnsafePtr(), blocks.Length);
            }

            dstManager.AddBuffer<ChunkMeshVerts>(entity);
            dstManager.AddBuffer<ChunkMeshIndices>(entity);
            dstManager.AddBuffer<ChunkMeshUVs>(entity);

#if UNITY_EDITOR
            dstManager.SetName(entity, $"Chunk {chunkIndex}");
#endif
        }

        private void OnDrawGizmosSelected()
        {
            int3 worldPos = (int3)math.floor(transform.position);

            var cell = Grid3D.WorldToCell(worldPos);
            float3 cellSize = Grid3D.CellSize;
            float3 p = cell * cellSize;
            float3 center = p + (cellSize * .5f);
            Gizmos.DrawWireCube(center, cellSize);
        }
    }
}