using Unity.Entities;
using Unity.Mathematics;

using BlockGame.BlockWorld.ChunkMesh;

namespace BlockGame.BlockWorld
{
	public class UpdateAdjacentChunksSystem : SystemBase
	{
		EndSimulationEntityCommandBufferSystem _barrier;
		EntityQuery _updateQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			_barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			var mapSystem = World.GetOrCreateSystem<RegionRegistrySystem>();

			Dependency = mapSystem.AddReaderHandle(Dependency);

			var chunkMap = World.GetOrCreateSystem<RegionRegistrySystem>().GetChunkMap();

			var ecb = _barrier.CreateCommandBuffer().ToConcurrent();
			_barrier.CreateCommandBuffer().RemoveComponent<UpdateAdjacentChunks>(_updateQuery);

			Entities
				.WithStoreEntityQueryInField(ref _updateQuery)
				.WithReadOnly(chunkMap)
				.WithAll<UpdateAdjacentChunks>()
				.ForEach((int entityInQueryIndex, Entity e, in ChunkIndex chunkIndex) =>
				{
					for(int dirIndex = 0; dirIndex < 6; ++dirIndex)
					{
						var dir = GridUtil.Grid3D.CubeDirections[dirIndex];
						int3 adjIndex = chunkIndex + dir;

						var chunk = chunkMap.GetChunkFromChunkIndex(adjIndex);
						if (chunk == Entity.Null)
							continue;
						ecb.AddComponent<GenerateMesh>(entityInQueryIndex, chunk);
					}
				}).ScheduleParallel();


			_barrier.AddJobHandleForProducer(Dependency);

		}
	}
}
