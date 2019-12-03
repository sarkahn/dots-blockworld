using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace BlockWorld
{
    public struct ChunkHeight : IBufferElementData
    {
        public float value;
        public static implicit operator float(ChunkHeight e) => e.value;
        public static implicit operator ChunkHeight(float v) => new ChunkHeight { value = v };
    }
}
