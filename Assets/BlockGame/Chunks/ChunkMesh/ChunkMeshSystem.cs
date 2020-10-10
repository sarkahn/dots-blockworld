using BlockGame.VoxelWorld;
using GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
                .WithAll<UpdateChunkMesh>()
                //.WithReadOnly(voxelWorld)
                .WithStoreEntityQueryInField(ref _updateMeshQuery)
                .ForEach((int entityInQueryIndex, Entity e,
                ref DynamicBuffer<ChunkMeshVerts> verts,
                ref DynamicBuffer<ChunkMeshIndices> indices,
                ref DynamicBuffer<ChunkMeshUVs> uvs,
                in DynamicBuffer<VoxelChunkBlocks> blocksBuffer
                ) =>
            {
                var blocks = blocksBuffer.Reinterpret<ushort>().AsNativeArray();
                verts.Clear();
                for(int i = 0; i < blocks.Length; ++i)
                {
                    ushort block = blocks[i];
                    if (block == 0)
                        continue;

                    for(int dirIndex = 0; dirIndex < Grid3D.Orthogonal.Length; ++dirIndex)
                    {
                        int3 dir = Grid3D.Orthogonal[dirIndex];
                    }
                }
            }).ScheduleParallel();

            var ecb = _endSimBarrier.CreateCommandBuffer();
            ecb.RemoveComponent<UpdateChunkMesh>(_updateMeshQuery);

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        public static bool BlockIsOpaque(ushort blockType)
        {
            return blockType != 0;
        }
    }
}