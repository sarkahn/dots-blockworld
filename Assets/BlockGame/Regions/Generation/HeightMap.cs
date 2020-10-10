using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HeightMap : IBufferElementData
{
    public ushort height;
}
