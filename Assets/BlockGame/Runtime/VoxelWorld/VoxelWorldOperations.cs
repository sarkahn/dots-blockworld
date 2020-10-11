using Unity.Entities;
using Unity.Mathematics;

namespace Sark.BlockGame
{
    public class VoxelWorldOperations
    {
        public static Entity AddChunkToRegion(
            EntityCommandBuffer.ParallelWriter ecb,
            int entityInQueryIndex,
            int3 chunkIndex,
            Entity chunkPrefab,
            Entity region)
        {
            var chunk = ecb.Instantiate(entityInQueryIndex, chunkPrefab);

            ecb.SetComponent(entityInQueryIndex, chunk, new VoxelChunk
            {
                Index = chunkIndex,
                Region = region
            });

            ecb.AppendToBuffer<LinkedEntityGroup>(entityInQueryIndex, region, chunk);

            return chunk;
        }
    }
}