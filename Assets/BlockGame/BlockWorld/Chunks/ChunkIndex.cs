using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    public struct GridIndex3DShared : ISharedComponentData
    {
        public int3 value;
        public static implicit operator int3(GridIndex3DShared e) => e.value;
        public static implicit operator GridIndex3DShared(int3 v) => new GridIndex3DShared { value = v };
    }

}