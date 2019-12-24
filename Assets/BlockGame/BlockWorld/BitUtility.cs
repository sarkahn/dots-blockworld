using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

// Be aware of how much you can store in each data type. For instance a byte is 8 bits, meaning you could
// only store 2 separate 4 (2*4 = 8 bits) bit values. A 4 bit value is from 0 - 15 (max = 2 ^ numBits - 1)
// In a char ( 16 bits) you could store 4 3 bit values and 1 4 bit value. (4 * 3 = 12) + ( 4 ) = 16 bits.
namespace SUtils.Bitshifting
{
    public static class BitUtility
    {
        /// <summary>
        /// Store a value in the given storage uint (32 bits) at the given index. Remember to assign
        /// the return value to something.
        /// </summary>
        /// <param name="storage">The data you're writing to.</param>
        /// <param name="bitValue">Value to store.</param>
        /// <param name="bitIndex">Index to store the value at.</param>
        /// <param name="numBits">Number of bits required to store the value.</param>
        /// <returns>The original data value with the bit value stored in it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBitValue(this uint storage, uint bitValue, int index, int numBits)
        {
            uint mask = ((1U << numBits) - 1U) << index;
            return (storage & ~mask) | (bitValue << index);
        }

        /// <summary>
        /// Retrieve a bit value from the given uint (32 bits) at the given index.
        /// </summary>
        /// <param name="storage">The data you're reading from.</param>
        /// <param name="bitIndex">Index the value is stored at.</param>
        /// <param name="numBits">The number of bits required to store the value.</param>
        /// <returns>The value stored at the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBitValue(this uint storage, int bitIndex, int numBits)
        {
            uint mask = ((1U << numBits) - 1U) << bitIndex;
            return (storage & mask) >> bitIndex;
        }

        /// <summary>
        /// Store a bit value in the given byte (8 bits) at the given index. Remember to assign
        /// the return value to something.
        /// </summary>
        /// <param name="storage">The data we're writing to.</param>
        /// <param name="bitValue">Value to store.</param>
        /// <param name="bitIndex">Index to store the value at.</param>
        /// <param name="numBits">Number of bits required to store the value.</param>
        /// <returns>Original data with our bit value stored in it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SetBitValue(this byte storage, uint bitValue, int bitIndex, int numBits)
        {
            int shiftedMask = ((1 << numBits) - 1) << bitIndex;
            return (byte)((storage & ~shiftedMask) | ((int)bitValue << bitIndex));
        }

        /// <summary>
        /// Retrieve a bit value from the given byte (8 bits) at the given index.
        /// </summary>
        /// <param name="storage">The data we're reading from.</param>
        /// <param name="bitIndex">Index the value is stored at.</param>
        /// <param name="numBits">The number of bits required to store the value.</param>
        /// <returns>The value stored at the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBitValue(this byte storage, int bitIndex, int numBits)
        {
            int mask = ((1 << numBits) - 1) << bitIndex;
            return (storage & mask) >> bitIndex;
        }

        /// <summary>
        /// Store a bit value in the given storage char (16 bits) at the given index. Remember to assign
        /// the return value to something.
        /// </summary>
        /// <param name="storage">The data we're writing to.</param>
        /// <param name="bitValue">Value to store.</param>
        /// <param name="bitIndex">Index to store the value at.</param>
        /// <param name="numBits">Number of bits required to store the value.</param>
        /// <returns>Original data with our bit value stored in it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char SetBitValue(this char storage, uint bitValue, int bitIndex, int numBits)
        {
            int shiftedMask = ((1 << numBits) - 1) << bitIndex;
            return (char)((storage & ~shiftedMask) | ((int)bitValue << bitIndex));
        }

        /// <summary>
        /// Retrieve a bit value from the given storage char (16 bits) at the given index.
        /// </summary>
        /// <param name="storage">The storage value.</param>
        /// <param name="bitIndex">Index the value is stored at.</param>
        /// <param name="numBits">The number of bits required to store the value.</param>
        /// <returns>The value stored at the given index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBitValue(this char storage, int bitIndex, int numBits)
        {
            int mask = ((1 << numBits) - 1) << bitIndex;
            return (storage & mask) >> bitIndex;
        }
    }
}