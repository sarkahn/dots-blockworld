using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[InternalBufferCapacity(4)]
	public struct RegionNeighbours : IBufferElementData
	{
		public Entity value;
		public static implicit operator Entity(RegionNeighbours b) => b.value;
		public static implicit operator RegionNeighbours(Entity v) => new RegionNeighbours { value = v };
	} 
}
