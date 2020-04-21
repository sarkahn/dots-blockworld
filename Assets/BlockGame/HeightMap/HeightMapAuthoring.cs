using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class HeightMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        GenerateHeightmapSettings _settings = GenerateHeightmapSettings.Default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<HeightMapBuffer>(entity);
            buffer.ResizeUninitialized(Constants.ChunkSurfaceVolume);
            for (int i = 0; i < Constants.ChunkSurfaceVolume; ++i)
                buffer.Add(default);
        }

#if UNITY_EDITOR
        ushort[] _heightMap = null;

        [SerializeField]
        bool _drawHeightMap = false;

        [SerializeField]
        int2 _size = Constants.ChunkSurfaceSize;

        private void OnDrawGizmosSelected()
        {
            if (!_drawHeightMap)
                return;

            float3 p = transform.position;
            int2 worldOrigin = (int2)math.floor(p.xz);

            int len = _size.x * _size.y;

            if (_heightMap == null || _heightMap.Length != len)
            {
                _heightMap = new ushort[len];
                Debug.Log($"Initing new heightmap - map of size {_heightMap.Length}");
                HeightMapUtility.Build(worldOrigin, _size, _settings, new ManagedHeightMap(_heightMap));
            }

            for( int i = 0; i < _heightMap.Length; ++i )
            {
                float3 worldPos = new float3();
                worldPos.xz = worldOrigin + GridUtil.Grid2D.IndexToPos(i, _size.x);
                worldPos.y = _heightMap[i];

                Gizmos.DrawWireCube(worldPos + new float3(.5f), new float3(.85f));
            }
        }

        private void OnValidate()
        {
            if (!_drawHeightMap )
                return;

            float3 p = transform.position;
            int2 worldOrigin = (int2)math.floor(p.xz);
            int len = _size.x * _size.y;
            _heightMap = new ushort[len];
            HeightMapUtility.Build(worldOrigin, _size, _settings, new ManagedHeightMap(_heightMap));
        }
#endif
    } 
}