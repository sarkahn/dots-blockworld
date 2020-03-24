using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class GenerateRegionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float3 chunkPos = math.floor(new float3(transform.position) / Constants.ChunkSize);
            int2 pos = (int2)chunkPos.xz;
            dstManager.AddComponentData(entity,
                new GenerateRegion { regionIndex = pos });
        }
    } 
}
