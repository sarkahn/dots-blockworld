using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace BlockWorld
{
    public struct EntityBuffer : IBufferElementData
    {
        public Entity value;
        public static implicit operator Entity(EntityBuffer e) => e.value;
        public static implicit operator EntityBuffer(Entity v) => new EntityBuffer { value = v };
    }
}