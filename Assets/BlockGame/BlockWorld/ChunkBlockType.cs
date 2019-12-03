using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockWorld
{
    public struct ChunkBlockType : IBufferElementData
    {
        public int value;
        public static implicit operator int(ChunkBlockType t) => t.value;
        public static implicit operator ChunkBlockType(int v) => new ChunkBlockType { value = v };
    }
}
