using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    public struct RegionIndexShared : ISharedComponentData
    {
        public int2 value;
        public static implicit operator int2(RegionIndexShared e) => e.value;
        public static implicit operator RegionIndexShared(int2 v) => new RegionIndexShared { value = v };
    }

}