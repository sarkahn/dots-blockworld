using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	/// <summary>
	/// The 3 dimensional index of a given chunk of blocks. The x/z values match the region index
	/// this chunk is in.
	/// </summary>
	public struct ChunkIndex : IComponentData
	{
		public int3 value;
		public static implicit operator int3(ChunkIndex c) => c.value;
		public static implicit operator ChunkIndex(int3 v) => new ChunkIndex { value = v };
		public int2 RegionIndex => value.xz;
	} 
}
