using BlockWorld;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

// TODO : REWRITE
[TestFixture]
public class GridMathTests
{
    const int ChunkSizeX = Constants.ChunkSizeX;
    const int ChunkSizeY = Constants.ChunkSizeY;
    const int ChunkSizeZ = Constants.ChunkSizeZ;
    const int ChunkVolume = Constants.ChunkVolume;
    static int3 ChunkSize => new int3(ChunkSizeX, ChunkSizeY, ChunkSizeZ);

    [Test]
    public void LocalPosToArrayIndex()
    {

        HashSet<int> indices = new HashSet<int>();

        // Ensure all calculated indices are two-way and unique
        for (int x = 0; x < ChunkSizeX; ++x)
        {
            for (int y = 0; y < ChunkSizeY; ++y)
            {
                for (int z = 0; z < ChunkSizeZ; ++z)
                {
                    int3 pos = new int3(x, y, z);

                    int calcedIndex = GridMath.Grid3D.ArrayIndexFromCellPos(pos, ChunkSize);
                    int3 calcedPos = GridMath.Grid3D.CellPosFromArrayIndex(calcedIndex, ChunkSize);

                    Assert.False(indices.Contains(calcedIndex));

                    Assert.Less(calcedIndex, ChunkVolume);

                    Assert.GreaterOrEqual(calcedIndex, 0);

                    Assert.AreEqual(pos, calcedPos);

                    indices.Add(calcedIndex);
                }
            }
        }
    }

    [Test]
    public void GetCellPos()
    {        
        for( int i = 0; i < 10; ++i)
        {
            int3 p = new int3(i * ChunkSizeX, i * ChunkSizeY, i * ChunkSizeZ);
            int3 cellIndex = GridMath.Grid3D.CellIndex(p, ChunkSize);

            Assert.AreEqual(i, cellIndex.x);
        }
    }
}
