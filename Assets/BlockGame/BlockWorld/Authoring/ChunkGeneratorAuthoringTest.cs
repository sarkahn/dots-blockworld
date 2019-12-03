using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace BlockWorld.Authoring
{
    public class ChunkGeneratorAuthoringTest : MonoBehaviour, IConvertGameObjectToEntity
    {
        static int ChunkSize = 32;
        static int ChunkHeight = 20;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<ChunkHeight>(entity);
            dstManager.AddComponent<GenerateChunkTest>(entity);
            
            dstManager.SetComponentData<ChunkIndex>(entity, GetChunkIndex(transform.position));
            dstManager.SetComponentData<ChunkSize>(entity, new int2(ChunkSize));
        }

        private void OnDrawGizmosSelected()
        {
            var origin = GetChunkWorldOrigin(transform.position);
            Vector3 chunkSize = new Vector3(ChunkSize, ChunkHeight, ChunkSize);
            var chunkCenter = origin + chunkSize * .5f;

            Gizmos.DrawWireCube(chunkCenter, chunkSize);
        }
        
        int2 GetChunkIndex(Vector3 position3D) => 
            GetChunkIndex(((float3)position3D).xz);

        int2 GetChunkIndex(float2 xzPos)
        {
            xzPos /= new float2(ChunkSize);
            xzPos = math.floor(xzPos);
            return (int2)xzPos;
        }

        Vector3 GetChunkWorldOrigin(Vector3 position3D) => 
            GetChunkWorldOrigin(GetChunkIndex(position3D));

        Vector3 GetChunkWorldOrigin(int2 chunkIndex)
        {
            chunkIndex *= new int2(ChunkSize);
            return new Vector3(chunkIndex.x, 0, chunkIndex.y);
        }
    }
}
