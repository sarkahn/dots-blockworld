using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct HeightMap
	{
		public BlobArray<int> values;

		public int this[int i]
		{
			get => values[i];
			set => values[i] = value;
		}
	}

	public static class HeightMapBuilder
	{
		public static BlobAssetReference<HeightMap> BuildHeightMap(
			int2 worldPos, int2 size,
			GenerateRegionHeightMapSettings settings,
			Allocator allocator)
		{
			var builder = new BlobBuilder(Allocator.Temp);

			ref var root = ref builder.ConstructRoot<HeightMap>();

			var values = builder.Allocate(ref root.values, size.x * size.y);

			int iterations = settings.iterations;
			int minHeight = settings.minHeight;
			int maxHeight = settings.maxHeight;
			float persistence = settings.persistence;
			float scale = settings.scale;

			for (int x = 0; x < size.x; ++x)
				for (int z = 0; z < size.y; ++z)
				{
					float h = NoiseUtil.SumOctave(
						worldPos.x + x, worldPos.y + z,
						iterations, persistence, scale, minHeight, maxHeight);

					int2 localPos = new int2(x, z);
					int i = GridUtil.Grid2D.PosToIndex(localPos);
					values[i] = (int)math.floor(h);
				}

			var rootRef = builder.CreateBlobAssetReference<HeightMap>(allocator);

			return rootRef;
		}
	}

}