
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace BlockGame.BlockWorld.ChunkMesh
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

			//Entities.WithNone<RenderMesh>()
			//	.ForEach((int entityInQueryIndex, Entity e, in ChunkWorldHeight chunkHeight, in ChunkIndex chunkIndex) =>
			//	{
			//		var index = chunkIndex.value;
			//		float3 p = new float3(
			//			index.x * Constants.ChunkSizeX,
			//			chunkHeight,
			//			index.z * Constants.ChunkSizeZ);
			//		var renderMesh = new RenderMesh();
			//		buffer.AddSharedComponent(entityInQueryIndex, e, renderMesh);
			//		buffer.AddComponent<RenderBounds>(entityInQueryIndex, e);
			//		buffer.AddComponent<PerInstanceCullingTag>(entityInQueryIndex, e);
			//		buffer.AddComponent<LocalToWorld>(entityInQueryIndex, e);
			//		buffer.AddComponent<Translation>(entityInQueryIndex, e, new Translation
			//		{
			//			Value = p
			//		});
			//	}).ScheduleParallel();

			Entities
				.WithName("AddMeshDataToChunks")
				.WithAll<GenerateMesh>()
				.WithNone<ChunkMeshVerts>()
				.WithNone<ChunkMeshIndices>()
				.WithNone<ChunkMeshUVs>()
				//.WithNone<RenderMesh>()
				.ForEach((int entityInQueryIndex, Entity e, in ChunkWorldHeight chunkHeight, in ChunkIndex chunkIndex) =>
				{
					var index = chunkIndex.value;
					float3 p = new float3(
						index.x * Constants.ChunkSizeX,
						chunkHeight,
						index.z * Constants.ChunkSizeZ);

					buffer.AddBuffer<ChunkMeshVerts>(entityInQueryIndex, e);
					buffer.AddBuffer<ChunkMeshIndices>(entityInQueryIndex, e);
					buffer.AddBuffer<ChunkMeshUVs>(entityInQueryIndex, e);

				}).ScheduleParallel();

			_barrier.AddJobHandleForProducer(Dependency);
		}

		void GenerateMeshData()
		{
			var mapSystem = World.GetOrCreateSystem<RegionRegistrySystem>();
			Dependency = mapSystem.AddReaderHandle(Dependency);

			var blockUVMap = World.GetOrCreateSystem<BlockRegistrySystem>().GetBlockUVMap();
			var chunkMap = World.GetOrCreateSystem<RegionRegistrySystem>().GetChunkMap();

			Entities
				.WithReadOnly(chunkMap)
				.WithName("GenerateChunkMeshData")
				.WithoutBurst()
				//.WithAll<RenderMesh>()
				.WithAll<GenerateMesh>()
				.ForEach((int entityInQueryIndex,
						  Entity e,
						  ref DynamicBuffer<ChunkMeshVerts> vertsBuffer,
						  ref DynamicBuffer<ChunkMeshIndices> indicesBuffer,
						  ref DynamicBuffer<ChunkMeshUVs> uvBuffer,
						  in DynamicBuffer<ChunkBlockBuffer> blocksBuffer,
						  in ChunkWorldHeight chunkWorldHeight,
						  in ChunkIndex chunkIndex) =>
				{
					var blocks = blocksBuffer.AsNativeArray();

					vertsBuffer.Clear();
					indicesBuffer.Clear();
					uvBuffer.Clear();

					var verts = vertsBuffer.Reinterpret<float3>();
					var indices = indicesBuffer.Reinterpret<int>();
					var uvs = uvBuffer.Reinterpret<float2>();

					var accessor = chunkMap.GetBlockAccessor(chunkIndex);

					for (int i = 0; i < blocks.Length; ++i)
					{
						var curr = blocks[i];
						if (curr == 0)
							continue;

						int3 xyz = GridUtil.Grid3D.IndexToPos(i);

						for (int dirIndex = 0; dirIndex < GridUtil.Grid3D.CubeDirections.Length; ++dirIndex)
						{
							var dir = GridUtil.Grid3D.CubeDirections[dirIndex];
							var adj = GetBlockType(xyz, dir, dirIndex, blocks, accessor);

							if (!IsOpaque(adj))
							{
								ref var faceUVs = ref blockUVMap.GetUVs(curr, dirIndex);
								BuildFace(xyz, dir, verts, indices, uvs, ref faceUVs);
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
						  in ChunkIndex chunkIndex,
						  in ChunkWorldHeight height) =>
				{
					if (_goMap.TryGetValue(e, out var oldGO))
						GameObject.Destroy(oldGO);

					var mesh = new Mesh();

					mesh.SetVertices(vertBuffer.Reinterpret<float3>().AsNativeArray());
					mesh.SetIndices(indicesBuffer.Reinterpret<int>().AsNativeArray(), MeshTopology.Triangles, 0);
					mesh.SetUVs(0, uvBuffer.Reinterpret<float2>().AsNativeArray());

					mesh.RecalculateBounds();
					mesh.RecalculateNormals();
					//mesh.RecalculateTangents();
					var index = chunkIndex.value.xz;

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
		static bool IsOpaque(ChunkBlockBuffer block) => block.blockType != 0;

		static ChunkBlockBuffer GetBlockType(int3 p, int3 dir, 
			int dirIndex, NativeArray<ChunkBlockBuffer> blocks, RegionRegistrySystem.AdjacentBlocksAccessor accessor)
		{
			var adj = p + dir;

			int i = 0;

			if (adj.x >= 0 && adj.x < Constants.ChunkSizeX &&
				adj.y >= 0 && adj.y < Constants.ChunkSizeY &&
				adj.z >= 0 && adj.z < Constants.ChunkSizeZ)
			{
				i = GridUtil.Grid3D.PosToIndex(adj);
				return blocks[i];
			}

			adj = adj & (Constants.ChunkSize - 1);
			i = GridUtil.Grid3D.PosToIndex(adj);

			var adjBlocks = accessor.GetBlocks(dirIndex);

			if( !adjBlocks.IsCreated )
				return 0;

			return adjBlocks[i];
		}
	}
}
