using NUnit.Framework;
using UnityEngine;

namespace Sark.BlockGame.Tests
{
    [TestFixture]
    public class ChunkMeshGOTests : VoxelWorldTestBase
    {
        [Test]
        public void ChunkGeneratesGO()
        {
            var world = VoxelWorld;

            world.SetBlock(0, 0, 0, 1);

            World.Update();

            var meshfilter = GameObject.FindObjectOfType<MeshFilter>();

            Assert.IsNotNull(meshfilter);
        }
    }
}