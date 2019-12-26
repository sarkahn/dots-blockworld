using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 CellPosFromArrayIndex(int i, int2 cellSize)
        {
            int x = i % cellSize.x;
            int y = (int)math.floor(i / cellSize.x);

            return new int2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 WorldPosFromCellIndex(int2 cellIndex, int2 cellSize)
        {
            var v = cellIndex * cellSize;
            return new float3(v.x, 0, v.y);
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 CellIndexFromWorldPos(float2 worldPos, int2 cellSize) => (int2)math.floor((float2)worldPos / cellSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<int2> CellsInRangeFromWorldPosition(int2 worldPos, int range, int2 cellSize, Allocator allocator)
            => CellsInRangeFromCellIndex(CellIndexFromWorldPos(worldPos, cellSize), range, cellSize, allocator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<int2> CellsInRangeFromCellIndex(int2 cellIndex, int range, int2 cellSize, Allocator allocator)
        {
            NativeList<int2> arr = new NativeList<int2>(allocator);

            if (range == 0)
                arr.Add(cellIndex);
            else
            {
                for (int x = -range; x <= range; ++x)
                        for (int z = -range; z <= range; ++z)
                            arr.Add(cellIndex + new int2(x, z));
            }

            return arr;
        }
        
        
    }

    public static class Grid3D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CellVolume(int3 cellSize) => cellSize.x * cellSize.y * cellSize.z;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CellVolumeXZ(int3 cellSize) => cellSize.x * cellSize.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CellIndexFromWorldPos(int3 worldPos, int3 cellSize) => (int3)math.floor((float3)worldPos / cellSize);

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


    public static NativeList<int3> GetCellsInRange(int3 cellIndex, int range, int3 cellSize, Allocator allocator)
    {
        NativeList<int3> arr = new NativeList<int3>(allocator);

        if (range == 0)
            arr.Add(cellIndex);
        else
        {
            for (int x = -range; x < range; ++x)
                for (int y = -range; y < range; ++y)
                    for (int z = -range; z < range; ++z)
                        arr.Add(cellIndex + new int3(x, y, z));
        }

        return arr;
    }
}


