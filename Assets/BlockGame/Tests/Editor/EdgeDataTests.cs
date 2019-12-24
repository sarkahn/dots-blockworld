using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace BlockWorld.EditorTests
{
    [TestFixture]
    public class EdgeDataTests
    {

        [Test]
        public void WriteBitsTest()
        {
            EdgeData e = new EdgeData();
            for ( int i = 0; i < 256; ++i )
            {
                e = e.WriteBit(i, true);
                Assert.AreEqual(true, e.ReadBit(i));
            }
        }

    }
}