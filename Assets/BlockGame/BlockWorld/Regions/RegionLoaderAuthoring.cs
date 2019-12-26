using BlockWorld;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld
{
    public struct RegionLoader : IComponentData
    {
        public int Range;
    }
    
    [RequiresEntityConversion]
    public class RegionLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Range = 2;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new RegionLoader
            {
                Range = Range,
            });
        }
        
        private void OnDrawGizmosSelected()
        {
            int2 chunkSize = Constants.Regions.Size;
            float3 pos = transform.position;
            var originCell = GridMath.Grid2D.CellIndexFromWorldPos(pos.xz, chunkSize);

            var cells = GridMath.Grid2D.CellsInRangeFromCellIndex(originCell, Range, chunkSize, Allocator.Temp);
            NativeArray<float3> worldPositions = new NativeArray<float3>(cells.Length, Allocator.Temp);

            // Convert indices to world positions
            for (int i = 0; i < cells.Length; ++i)
                worldPositions[i] = GridMath.Grid2D.WorldPosFromCellIndex(cells[i], chunkSize);

            float2 totalBoundsXZ = chunkSize * (Range * 2 + 1);
            float3 totalBounds = new float3(totalBoundsXZ.x, Constants.Regions.Height, totalBoundsXZ.y);
            float3 chunkBounds = new float3(chunkSize.x, Constants.Regions.Height, chunkSize.y);
            float3 originChunkCenter = GridMath.Grid2D.WorldPosFromCellIndex(originCell, chunkSize) + chunkBounds * .5f;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(originChunkCenter, totalBounds);

            Color transparentBlue = Color.blue;
            transparentBlue.a = .15f;
            Gizmos.color = transparentBlue;

            for (int i = 0; i < cells.Length; ++i)
            {
                float3 worldPos = worldPositions[i];
                float3 center = worldPos + chunkBounds * .5f;

                Gizmos.DrawWireCube(center, chunkBounds);
            }
        }
    }
}