using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld.Constants
{
    public static class BlockChunks
    {
        public const int SizeX = 16;
        public const int SizeY = 16;
        public const int SizeZ = 16;
        public const int Volume = SizeX * SizeY * SizeZ;
        public static int3 Size => new int3(SizeX, SizeY, SizeZ);
    }

    public static class Regions
    {
        public const int SizeX = BlockChunks.SizeX;
        public const int SizeZ = BlockChunks.SizeZ;
        public const int Volume = SizeX * SizeZ;
        public const int Height = 256;
        public static int2 Size => new int2(SizeX, SizeZ);
    }
}
