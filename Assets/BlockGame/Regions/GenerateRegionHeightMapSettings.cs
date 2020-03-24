using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [System.Serializable]
    public struct GenerateRegionHeightMapSettings : IComponentData
    {
        public int iterations;
        public int maxHeight;
        public int minHeight;
        public float scale;
        public float persistence;

        public static GenerateRegionHeightMapSettings Default => new GenerateRegionHeightMapSettings
        {
            iterations = 16,
            persistence = .5f,
            scale = 0.01f,
            minHeight = 0,
            maxHeight = 15,
        };
    }
}