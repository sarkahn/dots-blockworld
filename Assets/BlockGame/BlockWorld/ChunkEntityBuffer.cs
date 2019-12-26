using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace BlockWorld
{
    [System.Serializable]
    public struct ChunkEntityBuffer : IBufferElementData
    {
        public Entity value;
        public static implicit operator Entity(ChunkEntityBuffer e) => e.value;
        public static implicit operator ChunkEntityBuffer(Entity v) => new ChunkEntityBuffer { value = v };
    }
}