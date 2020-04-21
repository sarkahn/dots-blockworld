using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[GenerateAuthoringComponent]
	public struct GameChunk : IComponentData
	{
		/// <summary>
		/// The region this chunk entity exists in.
		/// </summary>
		public Entity parentRegion;
	} 
}
