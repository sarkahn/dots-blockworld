using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct HeightMapBuffer : IBufferElementData
{
    public float value;
    public static implicit operator float(HeightMapBuffer e) => e.value;
    public static implicit operator HeightMapBuffer(float v) => new HeightMapBuffer { value = v };
}
