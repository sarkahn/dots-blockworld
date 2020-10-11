using Unity.Entities;
using UnityEngine;

namespace Sark.BlockGame
{
    public struct RegionLoader : IComponentData
    {
        public int Range;
    }

    public class RegionLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Range;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new RegionLoader
            {
                Range = Range
            });
        }
    }
}