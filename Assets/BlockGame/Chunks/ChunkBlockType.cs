using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	//[InternalBufferCapacity(Constants.ChunkSizeX * Constants.ChunkSizeY * Constants.ChunkSizeZ)]
	// Force to allocate on the heap
	[InternalBufferCapacity(Constants.ChunkVolume)]
	[System.Serializable]
	[GenerateAuthoringComponent]
	public struct ChunkBlockType : IBufferElementData
	{
		public ushort blockType;

		public static implicit operator ChunkBlockType(int v) => new ChunkBlockType { blockType = (ushort)v };
		public static implicit operator int(ChunkBlockType c) => (int)c.blockType;
	}

}
