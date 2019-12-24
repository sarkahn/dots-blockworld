using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class GridMath 
{
    public static class Grid2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArrayIndexFromCellPos(int2 p, int2 size)
        {
            return p.y * size.x + p.x;
        }
        
    }

    public static class Grid3D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CellVolume(int3 cellSize) => cellSize.x * cellSize.y * cellSize.z;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CellVolumeXZ(int3 cellSize) => cellSize.x * cellSize.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CellIndex(int3 worldPos, int3 cellSize) => (int3)math.floor((float3)worldPos / cellSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArrayIndexFromWorldPos(int3 worldPos, int3 cellSize) =>
            ArrayIndexFromCellPos(CellPosFromWorldPos(worldPos, cellSize), cellSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArrayIndexFromCellPos(int3 cellPos, int3 cellSize) =>
            cellPos.x + cellSize.x * (cellPos.y + cellSize.y * cellPos.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CellPosFromArrayIndex(int i, int3 cellSize)
        {
            int z = i / (cellSize.x * cellSize.y);
            int y = (i - z * cellSize.x * cellSize.y) / cellSize.x;
            int x = i - cellSize.x * (y + cellSize.y * z);

            return new int3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CellPosFromWorldPos(int3 worldPos, int3 cellSize) => (worldPos % cellSize + cellSize) % cellSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldPosFromCellPos(int3 cellIndex, int3 cellPos, int3 cellSize) => cellIndex * cellSize + cellPos;

        /// <summary>
        /// The world position of the bottom left point of a given cell
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CellWorldOrigin(int3 cellIndex, int3 cellSize) => cellIndex * cellSize;
    }
}
