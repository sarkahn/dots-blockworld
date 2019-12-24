using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    [Serializable]
    public struct BlockBuffer : IBufferElementData
    {
        public Entity value;
        public static implicit operator Entity(BlockBuffer e) => e.value;
        public static implicit operator BlockBuffer(Entity v) => new BlockBuffer{ value = v };
    }
}