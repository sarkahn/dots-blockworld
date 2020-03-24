using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	//[InternalBufferCapacity(Constants.ChunkSizeX * Constants.ChunkSizeY * Constants.ChunkSizeZ)]
	// Force to allocate on the heap
	[InternalBufferCapacity(0)]
	[System.Serializable]
	[GenerateAuthoringComponent]
	public struct ChunkBlockType : IBufferElementData
	{
		public int blockType;
	}

}
