using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BlockGame.BlockWorld
{

    public class RegionRegistrySystem : SystemBase
    {
        NativeHashMap<int2, Entity> _regionMap;

        JobHandle _producerJob;

        EntityQuery _loadRegionQuery;
        EntityQuery _unloadRegionQuery;

        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _regionMap = new NativeHashMap<int2, Entity>(50000, Allocator.Persistent);

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _regionMap.Dispose(Dependency);
        }

        public JobHandle AddReaderHandle(JobHandle readerJob)
        {
            return JobHandle.CombineDependencies(Dependency, readerJob);
        }

        public RegionMap GetRegionMap() => new RegionMap(this);
        public ChunkMap GetChunkMap(bool isReadOnly = true) => new ChunkMap(this, isReadOnly);

        public AdjacentBlocksAccessor GetAdjacentBlocksAccessor(int3 chunkIndex) =>
            new AdjacentBlocksAccessor(GetChunkMap(true), chunkIndex);

        public bool TryGetRegion(int2 index, out Entity region) => _regionMap.TryGetValue(index, out region);

        protected override void OnUpdate()
        {
            _producerJob.Complete();

            var map = _regionMap;

            var ecb = _barrier.CreateCommandBuffer();

            int capacity = map.Capacity;
            if (map.Count() + _loadRegionQuery.CalculateEntityCount() >= capacity)
            {
                _regionMap.Dispose();
                _regionMap = new NativeHashMap<int2, Entity>(capacity << 1, Allocator.Persistent);
                map = _regionMap;
                Job.WithCode(() =>
                {
                    var kvps = map.GetKeyValueArrays(Allocator.Temp);
                    for( int i = 0; i < kvps.Keys.Length; ++i )
                    {
                        map.Add(kvps.Keys[i], kvps.Values[i]);
                    }
                }).Schedule();
            }

            Entities
                .WithStoreEntityQueryInField(ref _unloadRegionQuery)
                .WithAll<UnloadRegion>()
                .ForEach((Entity e, in RegionIndex regionIndex) =>
                {
                    map.Remove(regionIndex);
                }).Schedule();

            var writer = _regionMap.AsParallelWriter();

            Entities
                .WithStoreEntityQueryInField(ref _loadRegionQuery)
                .WithAll<LoadRegion>()
                .ForEach((Entity e, in RegionIndex regionIndex) =>
                {
                    writer.TryAdd(regionIndex.value, e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
        public struct RegionMap
        {
            NativeHashMap<int2, Entity> map;
            public RegionMap(RegionRegistrySystem mappingSystem)
            {
                this.map = mappingSystem._regionMap;
            }

            public Entity GetRegion(int2 index) => map[index];
        }

        public struct AdjacentBlocksAccessor
        {
            NativeArray<ChunkBlockBuffer> westBlocks;
            NativeArray<ChunkBlockBuffer> eastBlocks;
            NativeArray<ChunkBlockBuffer> upBlocks;
            NativeArray<ChunkBlockBuffer> downBlocks;
            NativeArray<ChunkBlockBuffer> northBlocks;
            NativeArray<ChunkBlockBuffer> southBlocks;

            public AdjacentBlocksAccessor(ChunkMap chunkMap, ChunkIndex chunkIndex)
            {
                upBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.Up);
                downBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.Down);
                westBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.West);
                eastBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.East);
                northBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.North);
                southBlocks = chunkMap.GetBlocksFromChunkIndex(chunkIndex + GridUtil.Grid3D.South);
            }

            public NativeArray<ChunkBlockBuffer> GetBlocks(int dirIndex)
            {
                switch(dirIndex)
                {
                    case 0: return upBlocks;
                    case 1: return downBlocks;
                    case 2: return westBlocks;
                    case 3: return eastBlocks;
                    case 4: return northBlocks;
                    case 5: return southBlocks;
                }
                return default;
            }
        }

        /// <summary>
        /// A helper struct to retrieve chunk or block data from any given world position.
        /// Preferred usage would be to retrieve the array of blocks once and use that 
        /// exclusively to access block data.
        /// </summary>
        public struct ChunkMap
        {
            NativeHashMap<int2, Entity> regionMap;
            BufferFromEntity<RegionChunksBuffer> chunksFromEntity;
            BufferFromEntity<ChunkBlockBuffer> blocksFromEntity;

            public AdjacentBlocksAccessor GetBlockAccessor(int3 chunkIndex)
            {
                return new AdjacentBlocksAccessor(this, chunkIndex);
            }

            public ChunkMap(RegionRegistrySystem system, bool isReadOnly = true)
            {
                this.regionMap = system._regionMap;
                this.chunksFromEntity = system.GetBufferFromEntity<RegionChunksBuffer>(isReadOnly);
                this.blocksFromEntity = system.GetBufferFromEntity<ChunkBlockBuffer>(isReadOnly);
            }

            public Entity GetChunkFromWorldPos(int3 worldPos) =>
                GetChunkFromChunkIndex(worldPos / Constants.ChunkSize);

            public NativeArray<ChunkBlockBuffer> GetBlocksFromWorldPos(int3 worldPos) =>
                GetBlocksFromChunkIndex(worldPos / Constants.ChunkSize);

            public Entity GetChunkFromChunkIndex(int3 chunkIndex)
            {
                if (!regionMap.TryGetValue(chunkIndex.xz, out var region))
                    return Entity.Null;
                //if (!chunksFromEntity.Exists(region))
                //    return Entity.Null;
                var chunks = chunksFromEntity[region];
                if ( chunkIndex.y < 0 || chunkIndex.y >= chunks.Length)
                    return Entity.Null;
                return chunks[chunkIndex.y];
            }

            public NativeArray<ChunkBlockBuffer> GetBlocksFromChunkIndex(int3 chunkIndex)
            {
                var chunk = GetChunkFromChunkIndex(chunkIndex);
                if (chunk == Entity.Null)
                    return default;
                return blocksFromEntity[chunk].AsNativeArray();
            }
        }

    } 
}
