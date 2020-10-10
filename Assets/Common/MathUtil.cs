using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;


namespace MathUtil
{
    public static class mathu
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int mod(int x, int period)
        {
            return ((x % period) + period) % period;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 mod(int2 xy, int2 period)
        {
            return ((xy % period) + period) % period;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 mod(int3 xyz, int3 period)
        {
            return ((xyz % period) + period) % period;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float roundincrement(float value, float increments)
        {
            increments = 1f / increments;
            return math.round(value * increments) / increments;
        }
    }
}