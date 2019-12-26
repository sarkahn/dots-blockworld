using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct GenerateHeightMap : IComponentData
{
    public int2 regionIndex;
}
