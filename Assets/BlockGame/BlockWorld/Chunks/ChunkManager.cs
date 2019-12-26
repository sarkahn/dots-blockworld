using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace BlockWorld
{
    public class ChunkManager : JobComponentSystem
    {
        public JobHandle FinalJobHandle = default;

        NativeHashMap<int3, Entity> _chunkMap;

        protected override void OnCreate()
        {
            _chunkMap = new NativeHashMap<int3, Entity>(100, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _chunkMap.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {



            FinalJobHandle = inputDeps;

            return inputDeps;
        }
    }
}