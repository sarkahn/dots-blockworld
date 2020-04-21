using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct RegionLoader : IComponentData
	{
		public int range;
		public int2 regionIndex;
		public GenerateHeightmapSettings heightMapSettings;
	}

	public class RegionLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField]
		int _range = 5;

		public GenerateHeightmapSettings _heightMapSettings = GenerateHeightmapSettings.Default;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new RegionLoader
			{
				range = _range,
				heightMapSettings = _heightMapSettings
			});
		}
	}
}
