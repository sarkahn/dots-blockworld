using Unity.Entities;
using Unity.Mathematics;

using BlockGame.BlockWorld.ChunkMesh;

namespace BlockGame.BlockWorld
{
    public class GenerateChunkSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;
        EntityQuery _generatingChunks;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _generatingChunks = GetEntityQuery(
                ComponentType.ReadOnly<GenerateChunk>()
                );
        }

        struct BlockIDs
        {
            public int sand;
            public int dirt;
            public int grass;
            public int stone;
        }

        BlockIDs GetBlockIDs()
        {
            var ids = new BlockIDs();
            var registry = World.GetOrCreateSystem<BlockRegistrySystem>();
            ids.dirt = registry.GetBlockID("Dirt");
            ids.sand = registry.GetBlockID("Sand");
            ids.grass = registry.GetBlockID("Grass");
            ids.stone = registry.GetBlockID("Stone");
            return ids;
        }

        static ChunkBlockBuffer SelectBlock(int height, int maxHeight, ref BlockIDs ids)
        {
            ChunkBlockBuffer block = default;

            int sandHeight = 4;
            int dirtHeight = 7;
            int grassHeight = 10;
            int stoneHeight = int.MaxValue;

            if (height <= sandHeight)
                block = ids.sand;
            else if (height <= dirtHeight )
                block = ids.dirt;
            else if (height <= grassHeight )
            {
                if (height == maxHeight)
                    block = ids.grass;
                else
                    block = ids.dirt;
            }
            else if (height < stoneHeight )
                block = ids.stone;

            return block;
        }

        protected override void OnUpdate()
        {
            var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();

            Entities
                .WithName("AddBlockBuffersToChunks")
                .WithAll<GenerateChunk>()
                .WithNone<ChunkBlockBuffer>()
                .ForEach((int entityInQueryIndex, Entity e) =>
                {
                    commandBuffer.AddBuffer<ChunkBlockBuffer>(entityInQueryIndex, e);
                }).ScheduleParallel();

            var blockIDs = GetBlockIDs();

            var hmFromEntity = GetBufferFromEntity<HeightMapBuffer>(true);

            Entities
                .WithName("InitializeChunkBlocks")
                .WithAll<GenerateChunk>()
                .ForEach((int entityInQueryIndex, Entity e, ref DynamicBuffer<ChunkBlockBuffer> blocksBuffer, 
                in ChunkWorldHeight chunkWorldHeight, in RegionHeightMap heightMapBlob, in ChunkIndex chunkIndex,
                in GameChunk chunk) =>
                {
                    blocksBuffer.ResizeUninitialized(Constants.ChunkVolume);

                    var blocks = blocksBuffer.AsNativeArray();

                    ref var heightMap = ref heightMapBlob.Array;

                    for( int i = 0; i < blocks.Length; ++i )
                    {
                        int3 xyz = GridUtil.Grid3D.IndexToPos(i);
                        int x = xyz.x;
                        int y = xyz.y;
                        int z = xyz.z;

                        int xzIndex = GridUtil.Grid2D.PosToIndex(x, z);

                        int maxHeight = heightMap[xzIndex];
                        int height = y + chunkWorldHeight;

                        if (height <= maxHeight)
                            blocks[i] = SelectBlock(height, maxHeight, ref blockIDs);
                        else
                            blocks[i] = default;
                    }

                    commandBuffer.RemoveComponent<GenerateChunk>(entityInQueryIndex, e);
                    commandBuffer.AddComponent<GenerateMesh>(entityInQueryIndex, e);
                }).ScheduleParallel();


            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}