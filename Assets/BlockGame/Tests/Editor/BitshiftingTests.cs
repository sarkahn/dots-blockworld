using NUnit.Framework;
using SUtils.Bitshifting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockWorld.EditorTests
{
    [TestFixture]
    public class BitShiftingTests
    {

        [Test]
        public void WriteBitsTest()
        {
            uint a = 0;
            HashSet<uint> cache = new HashSet<uint>();

            for( int i = 0; i < 32; ++i )
            {
                a = a.SetBitValue(1, i, 1);
                Assert.AreEqual(1, a.GetBitValue(i, 1));
                Assert.False(cache.Contains(a));
                cache.Add(a);
            }

            Assert.AreEqual(uint.MaxValue, a);
        }

    }
}