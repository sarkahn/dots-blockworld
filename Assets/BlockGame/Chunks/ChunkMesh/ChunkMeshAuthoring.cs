using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld.ChunkMesh
{
	public class ChunkMeshAuthoring : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<ChunkMeshIndices>(entity);
			dstManager.AddBuffer<ChunkMeshUVs>(entity);
			dstManager.AddBuffer<ChunkMeshVerts>(entity);
		}
	}
}
