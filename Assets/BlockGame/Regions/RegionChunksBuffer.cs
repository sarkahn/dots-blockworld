using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;

namespace BlockGame.BlockWorld
{
	[InternalBufferCapacity(2)]
	public struct RegionChunksBuffer : IBufferElementData
	{
		public Entity value;
		public static implicit operator Entity(RegionChunksBuffer c)=> c.value;
		public static implicit operator RegionChunksBuffer(Entity v)=> new RegionChunksBuffer { value = v };
	}
}
