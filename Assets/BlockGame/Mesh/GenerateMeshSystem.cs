using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[UpdateAfter(typeof(GenerateChunkSystem))]
	public class GenerateMeshSystem : SystemBase
	{
		EndSimulationEntityCommandBufferSystem _barrier;

		Material _sharedMat;

		Dictionary<Entity, GameObject> _goMap = new Dictionary<Entity, GameObject>();

		protected override void OnCreate()
		{
			base.OnCreate();

			_barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			AddRenderMeshes();
			GenerateMeshData();
			CleanGameObjects();
		}

		void AddRenderMeshes()
		{
			var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

			Entities
				.WithName("AddRenderMeshesToChunks")
				.WithAll<GenerateMesh>()
				.WithNone<ChunkMeshVerts>()
				.WithNone<ChunkMeshIndices>()
				.WithNone<ChunkMeshUVs>()
				//.WithNone<RenderMesh>()
				.ForEach((int entityInQueryIndex, Entity e, in ChunkWorldHeight chunkHeight, in RegionIndex regionIndex) =>
				{
					float3 p = new float3(
						regionIndex.value.x * Constants.ChunkSizeX,
						chunkHeight,
						regionIndex.value.y * Constants.ChunkSizeZ);

					//var renderMesh = new RenderMesh();
					//buffer.AddSharedComponent(entityInQueryIndex, e, renderMesh);
					//buffer.AddComponent<RenderBounds>(entityInQueryIndex, e);
					//buffer.AddComponent<PerInstanceCullingTag>(entityInQueryIndex, e);
					//buffer.AddComponent<LocalToWorld>(entityInQueryIndex, e);
					//buffer.AddComponent<Translation>(entityInQueryIndex, e, new Translation
					//{
					//	Value = p
					//});

					buffer.AddBuffer<ChunkMeshVerts>(entityInQueryIndex, e);
					buffer.AddBuffer<ChunkMeshIndices>(entityInQueryIndex, e);
					buffer.AddBuffer<ChunkMeshUVs>(entityInQueryIndex, e);

				}).ScheduleParallel();

			_barrier.AddJobHandleForProducer(Dependency);
		}

		void GenerateMeshData()
		{
			var blockUVMap = World.GetOrCreateSystem<BlockRegistrySystem>().GetBlockUVMap();

			Entities
				.WithName("GenerateChunkMeshData")
				//.WithAll<RenderMesh>()
				.WithAll<GenerateMesh>()
				.ForEach((int entityInQueryIndex,
						  Entity e,
						  ref DynamicBuffer<ChunkMeshVerts> vertsBuffer,
						  ref DynamicBuffer<ChunkMeshIndices> indicesBuffer,
						  ref DynamicBuffer<ChunkMeshUVs> uvBuffer,
						  in DynamicBuffer<ChunkBlockType> blocksBuffer) =>
				{
					var blocks = blocksBuffer.AsNativeArray();

					vertsBuffer.Clear();
					indicesBuffer.Clear();
					uvBuffer.Clear();

					var verts = vertsBuffer.Reinterpret<float3>();
					var indices = indicesBuffer.Reinterpret<int>();
					var uvs = uvBuffer.Reinterpret<float2>();

					for (int x = 0; x < Constants.ChunkSizeX; ++x)
					{
						for (int y = 0; y < Constants.ChunkSizeY; ++y)
						{
							for (int z = 0; z < Constants.ChunkSizeZ; ++z)
							{
								int3 p = new int3(x, y, z);
								int posIndex = GridUtil.Grid3D.PosToIndex(p);

								var curr = blocks[posIndex];
								if (curr.blockType == 0)
									continue;

								for (int dirIndex = 0; dirIndex < GridUtil.Grid3D.Directions.Length; ++dirIndex)
								{
									var dir = GridUtil.Grid3D.Directions[dirIndex];
									var adj = GetBlockType(p + dir, blocks);

									if( !IsOpaque(adj))
									{
										ref var faceUVs = ref blockUVMap.GetUVs(curr.blockType, dirIndex);

										BuildFace(p, dir, verts, indices, uvs, ref faceUVs);
									}
								}
							}
						}
					}
				}).ScheduleParallel();

			ApplyMeshData();
		}

		/// <summary>
		/// Create the mesh from our mesh data.
		/// </summary>
		void ApplyMeshData()
		{
			var commandBuffer = _barrier.CreateCommandBuffer();
			Entities
				.WithStructuralChanges()
				.WithAll<GenerateMesh>()
				//.WithAll<RenderMesh>()
				.ForEach((Entity e,
						  in DynamicBuffer<ChunkMeshVerts> vertBuffer,
						  in DynamicBuffer<ChunkMeshIndices> indicesBuffer,
						  in DynamicBuffer<ChunkMeshUVs> uvBuffer,
						  in RegionIndex regionIndex,
						  in ChunkWorldHeight height) =>
				{
					var mesh = new Mesh();

					mesh.SetVertices(vertBuffer.Reinterpret<float3>().AsNativeArray());
					mesh.SetIndices(indicesBuffer.Reinterpret<int>().AsNativeArray(), MeshTopology.Triangles, 0);
					mesh.SetUVs(0, uvBuffer.Reinterpret<float2>().AsNativeArray());

					mesh.RecalculateBounds();
					mesh.RecalculateNormals();
					//mesh.RecalculateTangents();

					int2 index = regionIndex.value;

					var go = new GameObject($"Chunk ({index.x}, {index.y}): {height.value / Constants.ChunkHeight}");
					var filter = go.AddComponent<MeshFilter>();
					var renderer = go.AddComponent<MeshRenderer>();
					filter.sharedMesh = mesh;
					if (_sharedMat == null)
						_sharedMat = Resources.Load<Material>("Materials/BlocksMat");
					renderer.sharedMaterial = _sharedMat;

					float3 p = new float3(index.x, height, index.y);
					p.xz *= Constants.ChunkSize.xz;
					go.transform.position = p;

					go.isStatic = true;

					commandBuffer.AddComponent<ChunkMeshGameObjectState>(e);
					_goMap[e] = go;

					//var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
					//renderMesh.mesh = mesh;
					//renderMesh.material = new Material(Shader.Find("Standard"));

					//EntityManager.SetSharedComponentData(e, renderMesh);
					//EntityManager.SetComponentData<RenderBounds>(e, new RenderBounds
					//{
					//	Value = mesh.bounds.ToAABB()
					//});

					commandBuffer.RemoveComponent<GenerateMesh>(e);
				}).Run();

			_barrier.AddJobHandleForProducer(Dependency);
		}

		/// <summary>
		/// Destroy gameobjects attached to chunks that no longer exist
		/// </summary>
		void CleanGameObjects()
		{
			var ecb = _barrier.CreateCommandBuffer();
			Entities
				.WithoutBurst()
				.WithAll<ChunkMeshGameObjectState>()
				.WithNone<ChunkMeshVerts>()
				.ForEach((Entity e) =>
				{
					GameObject.Destroy(_goMap[e]);
					_goMap.Remove(e);
					ecb.RemoveComponent<ChunkMeshGameObjectState>(e);
				}).Run();
			_barrier.AddJobHandleForProducer(Dependency);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void BuildFace(float3 p, int3 dir, 
			DynamicBuffer<float3> verts, DynamicBuffer<int> indices, DynamicBuffer<float2> uvs,
			ref BlobArray<float2> faceUVs)
		{
			float3 center = p + new float3(.5f);
			float3 normal = dir;

			float3 up = new float3(0, 1, 0);
			if (normal.y != 0)
				up = new float3(-1, 0, 0);

			float3 front = center + normal * .5f;

			float3 perp1 = math.cross(normal, up);
			float3 perp2 = math.cross(perp1, normal);

			int start = verts.Length;

			verts.Add(front + (-perp1 + perp2) * .5f);
			verts.Add(front + (perp1 + perp2) * .5f);
			verts.Add(front + (-perp1 + -perp2) * .5f);
			verts.Add(front + (perp1 + -perp2) * .5f);

			// For a normal going in the negative Z direction (Quad visible to a forward facing camera):
			// 0---1
			// | / |
			// 2---3
			indices.Add(start + 0);
			indices.Add(start + 1);
			indices.Add(start + 2);
			indices.Add(start + 3);
			indices.Add(start + 2);
			indices.Add(start + 1);

			// Uv order set to match the default order of Unity's sprite UVs.
			uvs.Add(faceUVs[0]);
			uvs.Add(faceUVs[2]);
			uvs.Add(faceUVs[3]);
			uvs.Add(faceUVs[1]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsOpaque(ChunkBlockType block) => block.blockType != 0;

		static ChunkBlockType GetBlockType(int3 p, NativeArray<ChunkBlockType> blocks)
		{
			if (p.x >= 0 && p.x < Constants.ChunkSizeX &&
				p.y >= 0 && p.y < Constants.ChunkSizeY &&
				p.z >= 0 && p.z < Constants.ChunkSizeZ)
			{
				int i = GridUtil.Grid3D.PosToIndex(p);
				return blocks[i];
			}

			return default;
		}
	}
}
