using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    public static class Constants
    {
        public const int ChunkSizeX = 16;
        public const int ChunkSizeY = 16;
        public const int ChunkSizeZ = 16;
        public const int ChunkVolume = ChunkSizeX * ChunkSizeY * ChunkSizeZ;
        public static int3 ChunkSize => new int3(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
    }
}
