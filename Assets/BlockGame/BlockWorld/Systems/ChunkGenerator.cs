using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BlockWorld
{
    public class ChunkGenerator : JobComponentSystem
    {
        BeginInitializationEntityCommandBufferSystem bufferSystem;

        protected override void OnCreate()
        {
            bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var buffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

            inputDeps = Entities.ForEach((Entity e, 
                int entityInQueryIndex, ref DynamicBuffer<ChunkHeight> chunkHeight, in ChunkSize chunkSize) =>
            {
                buffer.RemoveComponent<GenerateChunkTest>(entityInQueryIndex, e);

                for( int x = 0; x < chunkSize.value.x; ++x )
                {
                    for( int y = 0; y <chunkSize.value.y; ++y )
                    {

                    }
                }

            }).Schedule(inputDeps);
            

            return inputDeps;
        }
    }
}
