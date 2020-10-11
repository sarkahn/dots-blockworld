using BlockGame.Chunks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.Regions
{
    /// <summary>
    /// A region is a stack of 1 or more <see cref="VoxelChunk"/>. A region contains chunks as Linked Entities.
    /// </summary>
    public struct Region : IComponentData
    {
        public int2 Index;
    }

    public struct RegionCreator
    {
        EntityArchetype _archetype;

        public RegionCreator(SystemBase system)
        {
            _archetype = system.EntityManager.CreateArchetype(
                typeof(Region),
                typeof(VoxelChunkStack),
                typeof(LinkedEntityGroup));
        }

        public Entity CreateRegion(EntityCommandBuffer ecb, int2 regionIndex)
        {
            var region = ecb.CreateEntity(_archetype);
            ecb.SetComponent(region, new Region
            {
                Index = regionIndex
            });
            ecb.AppendToBuffer<LinkedEntityGroup>(region, region);

            return region;
        }

        public void CreateRegion(ExclusiveEntityTransaction tr, NativeArray<Entity> output)
        {
            tr.CreateEntity(_archetype, output);
            for (int i = 0; i < output.Length; ++i)
            {
                var buffer = tr.GetBuffer<LinkedEntityGroup>(output[i]);
                buffer.Add(output[i]);
                tr.AddComponent(output[i], typeof(Disabled));
            }
        }
    }
}
