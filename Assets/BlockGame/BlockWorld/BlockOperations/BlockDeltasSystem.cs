using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    public class ApplyBlockDeltasSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //int3 chunkSize = Constants.BlockChunks.Size;
            //Entities
            //    .ForEach((ref DynamicBuffer<BlockDelta> deltaBuffer) =>
            //    {
            //        NativeList<int3> indices = new NativeList<int3>(Allocator.Temp);
            //        for(int i = 0; i < deltaBuffer.Length; ++i)
            //        {
            //            var delta = deltaBuffer[i];
            //            int3 chunkIndex = GridMath.Grid3D.CellIndexFromWorldPos(delta.WorldPos, chunkSize);
            //            if (!indices.Contains(chunkIndex))
            //                indices.Add(chunkIndex);
            //        }
            //    }).Run();

            //inputDeps = Entities
            //    .
            //    .ForEach((ref DynamicBuffer<BlockDelta> deltas) =>
            //{

            //}).Schedule(inputDeps);

            return inputDeps;
        }
    }
}
