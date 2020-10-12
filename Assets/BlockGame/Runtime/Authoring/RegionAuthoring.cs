using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    public class RegionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float3 pos = transform.position;
            int2 regionIndex = Grid2D.WorldToCell(pos.xz);
            dstManager.AddComponentData(entity, new Region { Index = regionIndex });

            var buffer = dstManager.AddBuffer<LinkedEntityGroup>(entity);
            buffer.Add(entity);

#if UNITY_EDITOR
            dstManager.SetName(entity, $"Region {regionIndex}");
#endif
        }

        private void OnDrawGizmosSelected()
        {
            float3 worldPos = transform.position;
            int3 index = Grid3D.WorldToCell(worldPos);
            index.y = 0;

            float3 size = Grid3D.CellSize;
            size.y = 64;
            float3 p = (float3)index * size;
            float3 center = p + (size * .5f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}