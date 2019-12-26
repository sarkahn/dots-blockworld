using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace BlockWorld
{
    public struct RegionsRequest : IComponentData
    {

    }

    //public struct RegionsRequest
    //{
    //    public JobHandle JobHandle;
    //    public NativeArray<int2> RegionIndices;
    //    public NativeList<Entity> RegionEntities;
    //} 
}
