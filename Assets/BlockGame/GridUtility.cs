using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class GridUtil
{
    public static class Grid3D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int x, int y, int z) => x + Constants.ChunkSizeX * (y + Constants.ChunkSizeY * z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int3 p) => PosToIndex(p.x, p.y, p.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToPos(int i)
        {
            int w = Constants.ChunkSizeX;
            int h = Constants.ChunkSizeY;

            int z = i / (w * h);
            int y = (i - z * w * h) / w;
            int x = i - w * (y + h * z);

            return new int3(x, y, z);
        }

        public static readonly int3 Up =        new int3(0, 1, 0);
        public static readonly int3 Down =      new int3(0, -1, 0);
        public static readonly int3 West =      new int3(-1, 0, 0);
        public static readonly int3 East =     new int3(1, 0, 0);
        public static readonly int3 North =   new int3(0, 0, 1);
        public static readonly int3 South =      new int3(0, 0, -1);

        public static readonly int3[] CubeDirections = new int3[6]
        {
            new int3(0, 1, 0),  // Up
            new int3(0, -1, 0), // Down
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };

        /// <summary>
        /// List of directions along the x/y plane
        /// </summary>
        public static readonly int3[] HorizontalDirections = new int3[4]
        {
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };
    }

    public static class Grid2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int x, int z) => z * Constants.ChunkSizeX + x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 IndexToPos(int i)
        {
            int x = i % Constants.ChunkSizeX;
            int y = i / Constants.ChunkSizeX;
            return new int2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 IndexToPos(int i, int sizeX )
        {
            int x = i % sizeX;
            int y = i / sizeX;
            return new int2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int2 pos) => PosToIndex(pos.x, pos.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TaxicabDistance(int2 a, int2 b) =>
            math.abs(b.x - a.x) + math.abs(b.y - a.y);

        public static readonly int2 Up =    new int2(0, 1);
        public static readonly int2 Down =  new int2(0, -1);
        public static readonly int2 Right = new int2(1, 0);
        public static readonly int2 Left =  new int2(-1, 0);

        public static readonly int2[] Directions = new int2[]
        {
            new int2(0, 1),  // Up
            new int2(0, -1), // Down
            new int2(1, 0),  // Right
            new int2(-1, 0), // Left
        };
    }
}
