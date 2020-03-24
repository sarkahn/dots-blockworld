using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[RequireComponent(typeof(FlyingCameraController))]
	public class CameraEntityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentObject(entity, transform);
			dstManager.AddComponentObject(entity, GetComponent<FlyingCameraController>());
		}
	}
}
