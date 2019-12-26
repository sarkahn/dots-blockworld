using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    [Serializable]
    public struct BlockChunkIndex : ISharedComponentData
    {
        public int3 value;
        public static implicit operator int3(BlockChunkIndex t) => t.value;
        public static implicit operator BlockChunkIndex(int3 v) => new BlockChunkIndex { value = v };
    }
}
