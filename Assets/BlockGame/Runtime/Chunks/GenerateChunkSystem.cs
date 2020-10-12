using Sark.Common;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    public struct GenerateChunk : IComponentData
    { }

    class GenerateChunkSystem : SystemBase
    {
        EntityQuery _generatingChunks;

        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var hmFromEntity = GetBufferFromEntity<HeightMap>(true);

            Entities
                .WithAll<GenerateChunk>()
                .WithReadOnly(hmFromEntity)
                .WithStoreEntityQueryInField(ref _generatingChunks)
                .ForEach((
                ref DynamicBuffer<VoxelChunkBlocks> blocksBuffer,
                in VoxelChunk chunk) =>
            {
                var heightMap = hmFromEntity[chunk.Region].Reinterpret<ushort>().AsNativeArray();

                var blocks = blocksBuffer.Reinterpret<ushort>().AsNativeArray();

                for( int i = 0; i < heightMap.Length; ++i )
                {
                    var top = heightMap[i];

                    int2 xz = Grid2D.IndexToPos(i);

                    for( int height = 0; height + chunk.WorldHeight < top; ++height )
                    {
                        int3 xyz = new int3(xz.x, height, xz.y);
                        int blockIndex = Grid3D.LocalToIndex(xyz);
                        blocks[blockIndex] = 1;
                    }
                }
            }).ScheduleParallel();

            var ecb = _barrier.CreateCommandBuffer();
            ecb.AddComponent<RebuildChunkMesh>(_generatingChunks);
            ecb.RemoveComponent<GenerateChunk>(_generatingChunks);

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
