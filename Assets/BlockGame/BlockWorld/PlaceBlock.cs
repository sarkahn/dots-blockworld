using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlaceBlock : IComponentData
{
    public int3 pos;
    public int blockType;
}
