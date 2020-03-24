using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;



public static class NoiseUtil
{

    // https://cmaher.github.io/posts/working-with-simplex-noise/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SumOctave(int x, int y, int iterations, float persistence, float scale, int low, int high)
    {
        float maxAmp = 0;
        float amp = 1;
        float freq = scale;
        float v = 0;

        for (int i = 0; i < iterations; ++i)
        {
            v += noise.snoise(new float2(x * freq, y * freq)) * amp;
            maxAmp += amp;
            amp *= persistence;
            freq *= 2;
        }

        v /= maxAmp;

        v = v * (high - low) / 2f + (high + low) / 2f;

        return v;
    }
}
