using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    public struct RegionIndexBuffer : IBufferElementData
    {
        public int2 value;
        public static implicit operator int2(RegionIndexBuffer e) => e.value;
        public static implicit operator RegionIndexBuffer(int2 v) => new RegionIndexBuffer { value = v };
    }

}