using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using SUtils.Bitshifting;
using UnityEngine.Assertions;

public static class EdgeDataExtension
{
    /// <summary>
    /// Writes a single bit (true or false) for the given index in the
    /// <see cref="EdgeData"/>.
    /// </summary>
    /// <param name="data">The edge data we're writing to.</param>
    /// <param name="index">The bit index (0-255) we're writing to.</param>
    /// <param name="bit">The bit value.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EdgeData WriteBit(this EdgeData data, int index, bool bit)
    {
        Assert.IsTrue(index >= 0 && index < 256, "Index must be in range [0..255]");

        uint4x2 u4x2 = data.value;
        int x = (int)math.floor(index / 128);
        int y = (int)math.floor(index / 32) % 4;
        int bitIndex = index % 32;
        u4x2 = u4x2[x][y].SetBitValue(1, bitIndex, 1);
        return u4x2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBit(this EdgeData data, int index)
    {
        Assert.IsTrue(index >= 0 && index < 256, "Index must be in range [0..255]");

        uint4x2 u4x2 = data.value;
        int x = (int)math.floor(index / 128);
        int y = (int)math.floor(index / 32) % 4;
        int bitIndex = index % 32;

        return u4x2[x][y].GetBitValue(bitIndex, 1) == 1;
    }
}
