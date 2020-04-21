using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [InternalBufferCapacity(Constants.ChunkSurfaceVolume)]
    public struct HeightMapBuffer : IBufferElementData
    {
        public ushort height;
    }

    public interface IHeightMap
    {
        ushort this[int i] { get; set; }
    }

    public struct NativeHeightMap : IHeightMap
    {
        public NativeHeightMap(DynamicBuffer<HeightMapBuffer> buffer)
        {
            map = buffer.Reinterpret<ushort>().AsNativeArray();
        }

        NativeArray<ushort> map;

        public ushort this[int i]
        {
            get => map[i];
            set => map[i] = value;
        }

    }

    public struct ManagedHeightMap : IHeightMap
    {
        IList<ushort> map;
        public ManagedHeightMap(IList<ushort> map)
        {
            this.map = map;
        }
        public ushort this[int i]
        {
            get
            {
                return map[i];
            }
            set
            {
                if (i < 0 || i >= map.Count)
                {
                    Debug.LogError($"Error setting value at index {i} of map. Map size is {map.Count}");
                }
                map[i] = value;
            }
        }

    } 
}