using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Hybrid;
using Unity.Mathematics;
using UnityEngine;

public struct RegionLoader : IComponentData
{
    public int Range;
}

public class RegionLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Range;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RegionLoader
        {
            Range = Range
        });
    }
}
