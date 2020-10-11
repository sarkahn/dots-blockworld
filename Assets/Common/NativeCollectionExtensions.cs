using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

namespace Sark.Common.CollectionExtensions
{
    public static class NativeCollectionExtensions
    {
        public static T PopLast<T>(this DynamicBuffer<T> buffer) where T : unmanaged
        {
            int end = buffer.Length - 1;
            var v = buffer[end];
            buffer.RemoveAt(end);
            return v;
        }
        
        public static T PopLast<T>(this NativeList<T> list) where T : unmanaged
        {
            int end = list.Length - 1;
            var v = list[end];
            list.RemoveAt(end);
            return v;
        }
    }
}