using Unity.Entities;
using Unity.Mathematics;

namespace Sark.BlockGame
{
    /// <summary>
    /// A region is a stack of 1 or more <see cref="VoxelChunk"/>. A region contains chunks as Linked Entities.
    /// </summary>
    public struct Region : IComponentData
    {
        public int2 Index;
    }
}
