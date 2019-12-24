using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockWorld
{
    /// Compressed edge data describing whether a given face on the edge of a chunk is connected to 
    /// an opaque face in the neighbouring chunk. Each chunk will have a set of 12 ulongs (96 bytes).
    /// Each index of the buffer refers to one half of a side, where even numbers start at block 0 and 
    /// odd numbers finish at the last block of a chunk:
    /// 0,1 : Right/East side (+x)
    /// 2,3 : Left/West side  (-x)
    /// 4,5 : Up              (+y)
    /// 6,7 : Down            (-y)
    /// 8,9 : Forward/North   (+z)
    /// 10,11: Backward/South (-z)
    /// Note this is assumes a 16x16x16 chunk (256 faces per edge)
    public class ChunkSharedEdgeData : IBufferElementData
    {
        public ulong value;
    }
}