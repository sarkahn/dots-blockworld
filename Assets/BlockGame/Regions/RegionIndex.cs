using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public struct RegionIndex : IComponentData
    {
        public int2 value;

        public static implicit operator int2(RegionIndex c) => c.value;
        public static implicit operator RegionIndex(int2 v) => new RegionIndex { value = v };
    } 
}
