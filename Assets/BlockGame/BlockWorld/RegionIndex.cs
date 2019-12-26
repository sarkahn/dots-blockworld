using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    public struct RegionIndex : ISharedComponentData
    {
        public int2 value;
        public static implicit operator int2(RegionIndex e) => e.value;
        public static implicit operator RegionIndex(int2 v) => new RegionIndex { value = v };
    }

}