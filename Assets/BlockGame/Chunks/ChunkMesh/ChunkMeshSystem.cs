using BlockGame.VoxelWorldNS;
using GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static BlockGame.VoxelWorldNS.VoxelWorldSystem;

namespace BlockGame.Chunks.Meshes
{
    public class ChunkMeshSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;
        EntityQuery _updateMeshQuery;

        protected override void OnCreate()
        {
            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var heightMapFromEntity = GetBufferFromEntity<HeightMap>(true);
            var voxelWorldSystem = World.GetOrCreateSystem<VoxelWorldSystem>();
            var voxelWorld = voxelWorldSystem.GetVoxelWorldReadOnly();

            JobHandle voxelWorldDeps = voxelWorldSystem.GetOutputDependency();
            Dependency = JobHandle.CombineDependencies(Dependency, voxelWorldDeps);

            Entities
                .WithAll<RebuildChunkMeshData>()
                .WithReadOnly(voxelWorld)
                .WithStoreEntityQueryInField(ref _updateMeshQuery)
                .ForEach((int entityInQueryIndex, Entity e,
                ref DynamicBuffer<ChunkMeshVerts> verts,
                ref DynamicBuffer<ChunkMeshIndices> indices,
                ref DynamicBuffer<ChunkMeshUVs> uvs,
                in DynamicBuffer<VoxelChunkBlocks> blocksBuffer,
                in VoxelChunk chunk
                ) =>
            {
                var blocks = blocksBuffer.Reinterpret<ushort>().AsNativeArray();

                verts.Clear();
                indices.Clear();
                uvs.Clear();

                var adj = voxelWorld.GetAdjacentChunkBlocks(chunk.Index);

                for(int i = 0; i < blocks.Length; ++i)
                {
                    ushort block = blocks[i];
                    if (block == 0)
                        continue;

                    int3 xyz = Grid3D.IndexToLocal(i);

                    for(int dirIndex = 0; dirIndex < Grid3D.Orthogonal.Length; ++dirIndex)
                    {
                        int3 dir = Grid3D.Orthogonal[dirIndex];
                        ushort adjBlock = GetAdjacentBlock(blocks, adj, xyz, dir, dirIndex);
                        if(adjBlock == 0)
                        {
                            //BuildFace(xyz, dir, verts, indices, uvs);
                        }
                    }
                }
            }).ScheduleParallel();

            var ecb = _endSimBarrier.CreateCommandBuffer();
            ecb.RemoveComponent<RebuildChunkMeshData>(_updateMeshQuery);
            ecb.AddComponent<RebuildChunkMesh>(_updateMeshQuery);

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        public static void BuildFace(
            float3 xyz, 
            int3 dir, 
            DynamicBuffer<float3> verts,
            DynamicBuffer<int> indices,
            DynamicBuffer<float2> uvs)
        {
            float3 center = xyz + new float3(.5f);
            float3 normal = dir;

            float3 up = new float3(0, 1, 0);
            if (normal.y != 0)
                up = new float3(-1, 0, 0);

            float3 front = center + normal * .5f;

            float3 perp1 = math.cross(normal, up);
            float3 perp2 = math.cross(perp1, normal);

            int start = verts.Length;

            verts.Add(front + (-perp1 + perp2) * .5f);
            verts.Add(front + (perp1 + perp2) * .5f);
            verts.Add(front + (-perp1 + -perp2) * .5f);
            verts.Add(front + (perp1 + -perp2) * .5f);

            // For a normal going in the negative Z direction (Quad visible to a forward facing camera):
            // 0---1
            // | / |
            // 2---3
            indices.Add(start + 0);
            indices.Add(start + 1);
            indices.Add(start + 2);
            indices.Add(start + 3);
            indices.Add(start + 2);
            indices.Add(start + 1);

            //// Uv order set to match the default order of Unity's sprite UVs.
            //uvs.Add(faceUVs[0]);
            //uvs.Add(faceUVs[2]);
            //uvs.Add(faceUVs[3]);
            //uvs.Add(faceUVs[1]);
        }

        public static bool BlockIsOpaque(ushort blockType)
        {
            return blockType != 0;
        }

        public static ushort GetAdjacentBlock(
            NativeArray<ushort> blocks,
            AdjacentChunkBlocks adj,
            int3 xyz,
            int3 dir,
            int dirIndex)
        {
            int3 adjPos = xyz + dir;
            if (Grid3D.InCellBounds(adjPos))
            {
                int blockIndex = Grid3D.LocalToIndex(adjPos);
                return blocks[blockIndex];
            }

            var adjBlocks = adj[dirIndex];
            if (!adjBlocks.IsCreated)
                return 0;

            // Convert to the adjacent chunk's local position
            int3 adjLocal = Grid3D.ToLocal(adjPos);
            int adjLocalIndex = Grid3D.LocalToIndex(adjLocal);

            return adjBlocks[adjLocalIndex];
        }


    }
}