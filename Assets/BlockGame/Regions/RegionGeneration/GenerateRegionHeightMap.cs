using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[InternalBufferCapacity(Constants.ChunkSurfaceVolume)]
	public struct GenerateRegionHeightMap : IBufferElementData
	{
		public int value;
		public static implicit operator int(GenerateRegionHeightMap c) => c.value;
		public static implicit operator GenerateRegionHeightMap(int v) => new GenerateRegionHeightMap { value = v };
	} 
}
