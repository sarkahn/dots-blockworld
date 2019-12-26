using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld
{
    [Serializable]
    public struct BlockBuffer : IBufferElementData
    {
        public short value;
        public static implicit operator short(BlockBuffer e) => e.value;
        public static implicit operator BlockBuffer(short v) => new BlockBuffer{ value = v };
    }
}