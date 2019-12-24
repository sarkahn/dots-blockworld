using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A 256 bit mask representing a single edge of a 16x16 chunk. See <see cref="EdgeDataExtension"/>
/// for usage.
/// </summary>
public struct EdgeData : IComponentData
{
    public uint4x2 value;
    public static implicit operator uint4x2(EdgeData c) => c.value;
    public static implicit operator EdgeData(uint4x2 v) => new EdgeData { value = v };
}
