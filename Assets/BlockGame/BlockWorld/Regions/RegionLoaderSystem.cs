using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BlockWorld
{
    [AlwaysSynchronizeSystem]
    public class RegionSystem : JobComponentSystem
    {
        Dictionary<int2, Entity> _regionMap = new Dictionary<int2, Entity>();
        NativeHashMap<int3, Entity> _chunkMap;

        NativeList<int2> _previousCells;

        EntityQuery _regionQuery;
        EntityQuery _chunkQuery;

        BeginSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate()
        {
            _previousCells = new NativeList<int2>(Allocator.Persistent);
            _chunkMap = new NativeHashMap<int3, Entity>(10, Allocator.Persistent);

            _ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _chunkQuery = GetEntityQuery(
                ComponentType.ReadWrite<BlockBuffer>(),
                ComponentType.ReadOnly<BlockChunkIndex>()
                );
        }

        protected override void OnDestroy()
        {
            _previousCells.Dispose();
            _chunkMap.Dispose();
        }

        struct GenerateChunksJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;

            [ReadOnly]
            public NativeArray<int3> ChunkIndices;
            
            NativeHashMap<int3, Entity>.ParallelWriter ChunkMap;

            public void Execute()
            {
                for( int i = 0; i < ChunkIndices.Length; ++i )
                {
                    var e = CommandBuffer.CreateEntity();
                    var b = CommandBuffer.AddBuffer<BlockBuffer>(e);
                    b.ResizeUninitialized(Constants.BlockChunks.Volume);
                    ChunkMap.TryAdd(ChunkIndices[i], e);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float3 pos = default;
            int range = default;

            float dt = Time.DeltaTime;
            Entities
                .WithoutBurst()
                .ForEach((Transform t, RegionLoaderAuthoring loaderMB, ref Translation translation, ref RegionLoader loaderComponent ) =>
                {
                    // Copy our transform back to the entity translation so we can move
                    // it at runtime
                    translation.Value = t.position;
                    pos = translation.Value;
                    range = loaderMB.Range;
                }).Run();

            if (range == 0)
                return default;

            int2 loaderXZ = (int2)math.floor(pos.xz);
            int2 loaderRegionIndex = GridMath.Grid2D.CellIndexFromWorldPos(loaderXZ, Constants.Regions.Size);

            var regionIndices = GridMath.Grid2D.CellsInRangeFromCellIndex(loaderRegionIndex, range, Constants.Regions.Size, Allocator.Temp);

            for( int i = 0; i < regionIndices.Length; ++i )
            {
                int2 xz = regionIndices[i];
                int2 targetRegionIndex = loaderRegionIndex + xz;

                Entity regionEntity;
                if (!_regionMap.TryGetValue(targetRegionIndex, out regionEntity))
                {
                    var em = EntityManager;
                    _regionMap[targetRegionIndex] = regionEntity = em.CreateEntity();
                    em.SetName(regionEntity, $"Region {targetRegionIndex.ToString()}");
                    em.AddSharedComponentData<RegionIndexShared>(regionEntity, targetRegionIndex);
                    em.AddComponentData(regionEntity, new GenerateHeightMap { regionIndex = targetRegionIndex });
                }

            }

            //NativeList<BlockDelta> deltas = new NativeList<BlockDelta>(Allocator.TempJob);


            // We need to generate chunks based on our heightmaps.
            // How to do it?
            // Should we pre-generate the chunks and then write?
            //   - How do we account for a case where we want to write to a chunk
            //   - that doesn't exist? IE: If we nerd pole straight up, how do we handle
            //   - the case where the column crosses chunks?
            //   - Or if an item wants to place a row of blocks that crosses multiple chunks (and regions)

            //   - My gut feeling is the "ApplyBlockDeltasBatchJob" - a job that takes a list
            //   - of block deltas and does everything - generate the chunks on the fly as deltas are
            //   - processed and and write the blocks to the newly generated chunks. We would need to be able to generate
            //   - chunks from within the job....how?
            
            //   - We can generate entities and buffers with a commandbuffer....

            // Should we generate the chunks on the fly as we're writing?

            //if(_regionQuery.CalculateEntityCount() != 0 )
            //{
            //    inputDeps = Entities
            //        .WithNone<GenerateHeightMap>()
            //        .WithAll<GeneratingRegion>()
            //        .WithNone<ChunkEntityBuffer>()
            //        .WithStoreEntityQueryInField(ref _regionQuery)
            //        .ForEach((
            //            int entityInQueryIndex,
            //            Entity e,
            //            in DynamicBuffer<HeightMapBuffer> heightBuffer) =>
            //        {
                        
            //        }).Schedule(inputDeps);
            //}

            if( Input.GetButtonDown("Fire1"))
            {

                inputDeps = Entities
                    .ForEach((ref DynamicBuffer<BlockBuffer> BlockBuffer) =>
                    {

                    }).Schedule(inputDeps);

                

                //var deltas = new NativeList<BlockDelta>(Allocator.TempJob);

                //for( int i = 0; i < 10; ++i )
                //{
                //    deltas.Add(new BlockDelta { BlockType = 1, WorldPos = new int3(i, 0, 0) });
                //}

                //inputDeps = new ApplyRegionBlockDeltasJob
                //{
                //    CommandBuffer = _ecbSystem.CreateCommandBuffer(),
                //    BlockBufferFromEntity = GetBufferFromEntity<BlockBuffer>(false),
                //    ChunkMap = _chunkMap,
                //    Deltas = deltas,
                //}.Schedule(inputDeps);

                //deltas.Dispose(inputDeps);

                //_ecbSystem.AddJobHandleForProducer(inputDeps);
            }



            _previousCells.ResizeUninitialized(regionIndices.Length);
            NativeArray<int2>.Copy(regionIndices, _previousCells);

            return default;
        }
    }
}
