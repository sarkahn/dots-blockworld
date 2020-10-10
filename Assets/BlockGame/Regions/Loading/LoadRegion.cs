using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.Regions
{
    public struct LoadRegion : IComponentData
    {
        public int2 Index;
    } 
}
