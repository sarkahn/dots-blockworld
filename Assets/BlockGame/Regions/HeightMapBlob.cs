using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct HeightMapBlob
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
		public static BlobAssetReference<HeightMapBlob> BuildHeightMap(
			int2 worldPos, int2 size,
			GenerateHeightmapSettings settings,
			Allocator allocator)
		{
			var builder = new BlobBuilder(Allocator.Temp);

			ref var root = ref builder.ConstructRoot<HeightMapBlob>();

			var values = builder.Allocate(ref root.values, size.x * size.y);

			int iterations = settings.iterations;
			int minHeight = settings.minHeight;
			int maxHeight = settings.maxHeight;
			float persistence = settings.persistence;
			float scale = settings.scale;

			for( int i = 0; i < values.Length; ++i )
			{
				int2 xz = GridUtil.Grid2D.IndexToPos(i);
				float h = NoiseUtil.SumOctave(
					worldPos.x + xz.x, worldPos.y + xz.y,
					iterations, persistence, scale, minHeight, maxHeight);

				values[i] = (int)math.floor(h);
			}

			var rootRef = builder.CreateBlobAssetReference<HeightMapBlob>(allocator);

			return rootRef;
		}
	}

}