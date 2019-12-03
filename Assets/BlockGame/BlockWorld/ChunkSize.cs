using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    public struct ChunkSize : IComponentData
    {
        public int2 value;
        public static implicit operator int2(ChunkSize t) => t.value;
        public static implicit operator ChunkSize(int2 v) => new ChunkSize { value = v };
    }
}