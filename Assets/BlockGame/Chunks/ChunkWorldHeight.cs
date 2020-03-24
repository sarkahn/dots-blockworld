using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    /// <summary>
    /// Represents the world height of the bottom layer of a chunk.
    /// </summary>
    [GenerateAuthoringComponent]
    public struct ChunkWorldHeight : IComponentData
    {
        public int value;
        public static implicit operator int(ChunkWorldHeight c) => c.value;
        public static implicit operator ChunkWorldHeight(int v) => new ChunkWorldHeight { value = v };
    } 
}
