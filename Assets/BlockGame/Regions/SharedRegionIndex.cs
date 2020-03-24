using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public struct SharedRegionIndex : ISharedComponentData, IEquatable<SharedRegionIndex>
    {
        public int2 value;

        public static implicit operator int2(SharedRegionIndex c) => c.value;
        public static implicit operator SharedRegionIndex(int2 v) => new SharedRegionIndex { value = v };

        public bool Equals(SharedRegionIndex other)
        {
            return value.x == other.value.x && value.y == other.value.y;
        }

        public override int GetHashCode()
        {
            ulong x = (ulong)value.x;
            ulong y = (ulong)value.y;

            x = (x | (x << 16)) & 0x0000FFFF0000FFFF;
            x = (x | (x << 8)) & 0x00FF00FF00FF00FF;
            x = (x | (x << 4)) & 0x0F0F0F0F0F0F0F0F;
            x = (x | (x << 2)) & 0x3333333333333333;
            x = (x | (x << 1)) & 0x5555555555555555;

            y = (y | (y << 16)) & 0x0000FFFF0000FFFF;
            y = (y | (y << 8)) & 0x00FF00FF00FF00FF;
            y = (y | (y << 4)) & 0x0F0F0F0F0F0F0F0F;
            y = (y | (y << 2)) & 0x3333333333333333;
            y = (y | (y << 1)) & 0x5555555555555555;

            int v = (int)(x | (y << 1));

            return v;
        }
    } 
}
