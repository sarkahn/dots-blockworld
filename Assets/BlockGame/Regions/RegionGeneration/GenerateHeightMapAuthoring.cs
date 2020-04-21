using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class GenerateHeightMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        GenerateHeightmapSettings _settings = GenerateHeightmapSettings.Default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, _settings);
        }
    } 
}
