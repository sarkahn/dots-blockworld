using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public static class HeightMapUtility
    {
        //public static void Build(int2 worldXZ, GenerateHeightmapSettings settings, NativeArray<ushort> heightMap)
        //{
        //    for (int i = 0; i < Constants.ChunkSurfaceVolume; ++i)
        //    {
        //        int2 xz = worldXZ + GridUtil.Grid2D.IndexToPos(i);
        //        float h = NoiseUtil.SumOctave(
        //            xz.x, xz.y, settings.iterations, settings.persistence, settings.scale,
        //            settings.minHeight, settings.maxHeight);
        //        heightMap[i] = (ushort)math.floor(h);
        //    }
        //}

        public static void Build<T>(int2 worldXZ, int2 size, GenerateHeightmapSettings settings, T map ) where T : IHeightMap
        {
            for (int i = 0; i < size.x * size.y; ++i)
            {
                int2 xz = worldXZ + GridUtil.Grid2D.IndexToPos(i, size.x);
                float h = NoiseUtil.SumOctave(
                    xz.x, xz.y, settings.iterations, settings.persistence, settings.scale,
                    settings.minHeight, settings.maxHeight);
                map[i] = (ushort)math.floor(h);
            }
        }
    } 
}
