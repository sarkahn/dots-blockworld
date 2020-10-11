using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    /// <summary>
    /// Provides random fast-ish access to adjacent blocks from a given chunk position.
    /// Should be retrieved though <see cref="VoxelWorld.GetAdjacentChunkBlocks(int3)"/>
    /// or <see cref="VoxelWorldReadOnly.GetAdjacentChunkBlocks(int3)"/>
    /// </summary>
    public struct AdjacentChunkBlocks
    {
        public NativeArray<ushort> Up;
        public NativeArray<ushort> Down;
        public NativeArray<ushort> West;
        public NativeArray<ushort> East;
        public NativeArray<ushort> North;
        public NativeArray<ushort> South;

        public int3 ChunkIndex { get; private set; }

        public NativeArray<ushort> this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return Up;
                    case 1: return Down;
                    case 2: return West;
                    case 3: return East;
                    case 4: return North;
                    case 5: return South;
                    default: return default;
                }
            }
        }

        public AdjacentChunkBlocks(VoxelWorldState world, int3 chunkIndex)
        {
            ChunkIndex = chunkIndex;

            West = GetBlocks(world, chunkIndex + Grid3D.West);
            East = GetBlocks(world, chunkIndex + Grid3D.East);
            North = GetBlocks(world, chunkIndex + Grid3D.North);
            South = GetBlocks(world, chunkIndex + Grid3D.South);
            Up = GetBlocks(world, chunkIndex + Grid3D.Up);
            Down = GetBlocks(world, chunkIndex + Grid3D.Down);
        }

        static NativeArray<ushort> GetBlocks(VoxelWorldState world, int3 pos)
        {
            if (!world.ChunkMap.TryGetValue(pos, out Entity chunk))
                return default;

            return world.BlocksFromEntity[chunk].Reinterpret<ushort>().AsNativeArray();
        }
    }
}