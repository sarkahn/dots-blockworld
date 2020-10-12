using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Sark.BlockGame
{
    [Serializable]
    public struct HeightMap : IBufferElementData
    {
        public ushort Value;
    }
}
