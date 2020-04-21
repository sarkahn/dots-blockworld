using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[InternalBufferCapacity(Constants.ChunkVolume)]
	[System.Serializable]
	[GenerateAuthoringComponent]
	public struct ChunkBlockBuffer : IBufferElementData
	{
		public ushort blockType;

		public static implicit operator ChunkBlockBuffer(int v) => new ChunkBlockBuffer { blockType = (ushort)v };
		public static implicit operator int(ChunkBlockBuffer c) => (int)c.blockType;
	}

}
