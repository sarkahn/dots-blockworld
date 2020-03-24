using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct RegionHeightMap : IComponentData
	{
		public BlobAssetReference<HeightMap> heightMapBlob;

		public ref BlobArray<int> Array => ref heightMapBlob.Value.values;
	} 
}
