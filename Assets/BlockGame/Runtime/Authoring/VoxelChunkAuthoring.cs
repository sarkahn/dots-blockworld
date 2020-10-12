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
        public List<int4> blocksToSet = new List<int4>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int3 chunkIndex = Grid3D.WorldToCell(transform.position);
            dstManager.AddComponentData(entity, new VoxelChunk { Index = chunkIndex });

            var blocks = dstManager.AddBuffer<VoxelChunkBlocks>(entity);
            blocks.ResizeUninitialized(Grid3D.CellVolume);
            unsafe
            {
                UnsafeUtility.MemClear(blocks.GetUnsafePtr(),  
                    blocks.Length * UnsafeUtility.SizeOf<VoxelChunkBlocks>());
            }

            dstManager.AddBuffer<ChunkMeshVerts>(entity);
            dstManager.AddBuffer<ChunkMeshIndices>(entity);
            dstManager.AddBuffer<ChunkMeshUVs>(entity);

            blocks = dstManager.GetBuffer<VoxelChunkBlocks>(entity);

            if (blocksToSet.Count != 0)
            {
                foreach(var op in blocksToSet)
                {
                    int3 p = op.xyz;
                    ushort block = (ushort)op.w;
                    int i = Grid3D.LocalToIndex(p);
                    blocks[i] = block;
                }

                dstManager.AddComponent<RebuildChunkMesh>(entity);
            }

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