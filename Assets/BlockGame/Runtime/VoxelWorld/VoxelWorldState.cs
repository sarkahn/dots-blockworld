
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.BlockGame
{
    public struct VoxelWorldState
    {
        public BufferFromEntity<VoxelChunkBlocks> BlocksFromEntity;
        public BufferFromEntity<LinkedEntityGroup> LinkedGroupFromEntity;

        public ComponentDataFromEntity<Region> RegionFromEntity;
        public ComponentDataFromEntity<VoxelChunk> ChunkFromEntity;

        public NativeHashMap<int2, Entity> RegionMap;
        public NativeHashMap<int3, Entity> ChunkMap;

        public VoxelWorldState(
            VoxelWorldSystem system,
            NativeHashMap<int2, Entity> regionMap,
            NativeHashMap<int3, Entity> chunkMap,
            bool readOnly = false)
        {
            RegionMap = regionMap;
            ChunkMap = chunkMap;

            BlocksFromEntity = system.GetBufferFromEntity<VoxelChunkBlocks>(readOnly);
            LinkedGroupFromEntity = system.GetBufferFromEntity<LinkedEntityGroup>(readOnly);

            ChunkFromEntity = system.GetComponentDataFromEntity<VoxelChunk>(readOnly);
            RegionFromEntity = system.GetComponentDataFromEntity<Region>(readOnly);
        }
    }
}