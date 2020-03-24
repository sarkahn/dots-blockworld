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

        public static int PosToIndex(int3 p, int2 sizeXY) => p.x + sizeXY.x * (p.y + sizeXY.y * p.z);

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

        public static readonly int3[] Directions = new int3[]
        {
            new int3(0, 1, 0),  // Up
            new int3(0, -1, 0), // Down
            new int3(-1, 0, 0), // Left
            new int3(1, 0, 0),  // Right
            new int3(0, 0, 1),  // Forward
            new int3(0, 0, -1), // Back
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
        public static int PosToIndex(int2 pos) => PosToIndex(pos.x, pos.y);

        public static readonly int2[] orthogonalDirections = new int2[]
        {
            new int2(0, 1), // Up
			new int2(0, -1), // Down
			new int2(-1, 0), // Left
			new int2(1, 0), // Right
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TaxicabDistance(int2 a, int2 b) =>
            math.abs(b.x - a.x) + math.abs(b.y - a.y);
    }
}
