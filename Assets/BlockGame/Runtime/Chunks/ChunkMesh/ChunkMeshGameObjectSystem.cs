using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using static Sark.Common.GridUtil;

namespace Sark.BlockGame
{
    struct HasGameObjectProxy : ISystemStateComponentData
    {
        public int3 ChunkIndex;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ChunkMeshSystem))]
    public class ChunkMeshGameObjectSystem : SystemBase
    {
        Dictionary<int3, GameObject> _gameObjectMap;

        EndSimulationEntityCommandBufferSystem _barrier;

        Material _material;

        protected override void OnCreate()
        {
            _gameObjectMap = new Dictionary<int3, GameObject>(10000);
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();

            Entities
                .WithoutBurst()
                .WithAll<ChunkMeshVerts>()
                .WithNone<HasGameObjectProxy>()
                .ForEach((Entity e, in VoxelChunk chunk) =>
                {
                    int3 chunkIndex = chunk.Index;
                    var go = new GameObject($"Chunk {chunkIndex}");
                    var filter = go.AddComponent<MeshFilter>();
                    filter.sharedMesh = new Mesh();
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = _material;
                    _gameObjectMap.Add(chunkIndex, go);

                    var t = go.transform;
                    t.position = (float3)(chunkIndex * Grid3D.CellSize);
                    ecb.AddComponent(
                        e, 
                        new HasGameObjectProxy { ChunkIndex = chunkIndex });
                }).Run();

            Entities
                .WithoutBurst()
                .WithNone<ChunkMeshVerts>()
                .ForEach((Entity e, in HasGameObjectProxy tracker) =>
                {
                    Object.Destroy(_gameObjectMap[tracker.ChunkIndex]);
                    _gameObjectMap.Remove(tracker.ChunkIndex);
                    ecb.RemoveComponent<HasGameObjectProxy>(e);
                }).Run();

            Entities
                .WithoutBurst()
                .WithAll<RebuildChunkMesh>()
                .ForEach((
                    in DynamicBuffer<ChunkMeshVerts> vertsBuffer,
                    in DynamicBuffer<ChunkMeshIndices> indicesBuffer,
                    in DynamicBuffer<ChunkMeshUVs> uvBuffer,
                    in VoxelChunk chunk) =>
                {
                    int3 chunkIndex = chunk.Index;
                    var go = _gameObjectMap[chunkIndex];
                    var mesh = go.GetComponent<MeshFilter>().sharedMesh;

                    var verts = vertsBuffer.Reinterpret<float3>().AsNativeArray();
                    var tris = indicesBuffer.Reinterpret<int>().AsNativeArray();
                    var uvs = uvBuffer.Reinterpret<float2>().AsNativeArray();

                    mesh.Clear();
                    mesh.SetVertices(verts);
                    mesh.SetIndices(tris, MeshTopology.Triangles, 0);
                    mesh.SetUVs(0, uvs);

                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                }).Run();
                
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}