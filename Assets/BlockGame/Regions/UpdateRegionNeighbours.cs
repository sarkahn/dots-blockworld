using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct UpdateRegionNeighbours : IComponentData
	{
		public int2 regionIndex;

		public static implicit operator int2(UpdateRegionNeighbours c) => c.regionIndex;
		public static implicit operator UpdateRegionNeighbours(int2 v) => new UpdateRegionNeighbours { regionIndex = v };
	} 
}
