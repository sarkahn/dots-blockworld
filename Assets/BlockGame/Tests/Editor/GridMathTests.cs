using BlockWorld;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace BlockWorld
{
    // TODO : REWRITE
    [TestFixture]
    public class GridMathTests
    {
        public class Grid2D
        {
            const int SizeX = 16;
            const int SizeY = 32;
            static int2 Size = new int2(SizeX, SizeY);
            const int Volume = SizeX * SizeY;

            [Test]
            public void LocalPosToArrayIndex()
            {
                HashSet<int> indices = new HashSet<int>();

                // Ensure all calculated indices are two-way and unique
                for (int x = 0; x < SizeX; ++x)
                {
                    for (int y = 0; y < SizeY; ++y)
                    {
                        int2 pos = new int2(x, y);

                        int calcedIndex = GridMath.Grid2D.ArrayIndexFromCellPos(pos, Size);
                        int2 calcedPos = GridMath.Grid2D.CellPosFromArrayIndex(calcedIndex, Size);

                        Assert.False(indices.Contains(calcedIndex));

                        Assert.Less(calcedIndex, Volume);

                        Assert.GreaterOrEqual(calcedIndex, 0);

                        Assert.AreEqual(pos, calcedPos);

                        indices.Add(calcedIndex);
                    }
                }
            }
        }

        public class Grid3D
        {

            const int ChunkSizeX = Constants.BlockChunks.SizeX;
            const int ChunkSizeY = Constants.BlockChunks.SizeY;
            const int ChunkSizeZ = Constants.BlockChunks.SizeZ;
            const int ChunkVolume = Constants.BlockChunks.Volume;
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
                for (int i = 0; i < 10; ++i)
                {
                    int3 p = new int3(i * ChunkSizeX, i * ChunkSizeY, i * ChunkSizeZ);
                    int3 cellIndex = GridMath.Grid3D.CellIndexFromWorldPos(p, ChunkSize);

                    Assert.AreEqual(i, cellIndex.x);
                }
            }
        }
    }
}