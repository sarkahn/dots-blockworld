using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public struct HeightMapChunkIndex : ISharedComponentData
{
    public int2 value;
    public static implicit operator int2(HeightMapChunkIndex c) => c.value;
    public static implicit operator HeightMapChunkIndex(int2 v) => new HeightMapChunkIndex { value = v };

}
