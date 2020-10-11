using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using MathUtil;

using static MathUtil.mathu;

namespace GridUtil
{
    public static class Grid3D
    {
        public const int CellSizeX = 16;
        public const int CellSizeY = 16;
        public const int CellSizeZ = 16;
        public static int3 CellSize => new int3(CellSizeX, CellSizeY, CellSizeZ);
        public const int CellVolume = CellSizeX * CellSizeY * CellSizeZ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LocalToIndex(int x, int y, int z) => x + CellSizeX * (y + CellSizeY * z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LocalToIndex(int3 p) => LocalToIndex(p.x, p.y, p.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToLocal(int i)
        {
            int w = CellSizeX;
            int h = CellSizeY;

            int z = i / (w * h);
            int y = (i - z * w * h) / w;
            int x = i - w * (y + h * z);

            return new int3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToLocal(int3 pos)
        {
            // Only works if CellSize == 16
            return pos & 15;
            //return mod(pos, CellSizeX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldToCell(float3 worldPos)
        {
            return (int3)(math.floor(worldPos / CellSize));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldToCell(int3 worldPos)
        {
            return WorldToCell((float3)worldPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WorldToIndex(int3 worldPos)
        {
            return LocalToIndex(ToLocal(worldPos));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WorldToIndex(float3 worldPos)
        {
            int3 floored = (int3)math.floor(worldPos);
            return LocalToIndex(ToLocal(floored));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InCellBounds(int3 localPos)
        {
            return math.any(localPos < 0) || math.any(localPos >= CellSize);
        }

        public static readonly int3 Up = new int3(0, 1, 0);
        public static readonly int3 Down = new int3(0, -1, 0);
        public static readonly int3 West = new int3(-1, 0, 0);
        public static readonly int3 East = new int3(1, 0, 0);
        public static readonly int3 North = new int3(0, 0, 1);
        public static readonly int3 South = new int3(0, 0, -1);

        /// <summary>
        /// List of the six directions on each axis of a cube
        /// </summary>
        public static readonly int3[] Orthogonal = new int3[6]
        {
            new int3(0, 1, 0),  // Up
            new int3(0, -1, 0), // Down
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };

        /// <summary>
        /// List of directions along the x/z plane
        /// </summary>
        public static readonly int3[] Horizontal = new int3[4]
        {
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };
    }

    public static class Grid2D
    {
        public const int CellSizeX = 16;
        public const int CellSizeZ = 16;
        static public int2 CellSize = new int2( CellSizeX, CellSizeZ );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToCell(int2 worldPos)
        {
            return WorldToCell((float2)worldPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToCell(float2 worldPos)
        {
            return (int2)(math.floor(worldPos / CellSize));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int x, int z) => z * CellSizeX + x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 IndexToPos(int i)
        {
            int x = i % CellSizeX;
            int y = i / CellSizeX;
            return new int2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 IndexToPos(int i, int sizeX)
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

        public static readonly int2 Up = new int2(0, 1);
        public static readonly int2 Down = new int2(0, -1);
        public static readonly int2 Right = new int2(1, 0);
        public static readonly int2 Left = new int2(-1, 0);

        public static readonly int2[] Directions = new int2[]
        {
            Up,//new int2(0, 1),  // Up
            Down,//new int2(0, -1), // Down
            Left,//new int2(1, 0),  // Right
            Right,//new int2(-1, 0), // Left
        };
    }
}
