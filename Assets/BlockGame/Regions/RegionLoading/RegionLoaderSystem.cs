using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class RegionLoaderSystem : SystemBase
	{
		EndInitializationEntityCommandBufferSystem _endInitBarrier;
		EntityArchetype _regionArchetype;

		EntityQuery _loadRegionQuery;
		EntityQuery _regionLoaderQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			_endInitBarrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

			_regionArchetype = EntityManager.CreateArchetype(
				typeof(Region), 
				typeof(LoadRegion), 
				typeof(RegionIndex),
				typeof(RegionChunksBuffer),
				typeof(LinkedEntityGroup)
				);

			_loadRegionQuery = GetEntityQuery(
				ComponentType.ReadOnly<LoadRegion>()
				);

			RequireForUpdate(_regionLoaderQuery);
		}

		static void GetPointsInRange(int2 start, NativeList<int2> points, int range)
		{
			for (int x = -range; x < range; ++x)
				for (int z = -range; z < range; ++z)
				{
					int2 p = start + new int2(x, z);
					//if (GridUtil.Grid2D.TaxicabDistance(start, p) > range)
					//	continue;
					if (!points.Contains(p))
						points.Add(p);
				}
		}

		protected override void OnUpdate()
		{
			var loadedPoints = GetLoadedPoints();

			UnloadRegions(loadedPoints);

			LoadRegions(loadedPoints);

			loadedPoints.Dispose(Dependency);

			_endInitBarrier.AddJobHandleForProducer(Dependency);
		}

		NativeList<int2> GetLoadedPoints()
		{
			NativeList<int2> loadedPoints = new NativeList<int2>(100, Allocator.TempJob);
			var writer = loadedPoints.AsParallelWriter();

			int range = GameObject.FindObjectOfType<MapGenerationSettingsUI>()._range;

			// Assign region loader indices
			Entities
				.WithStoreEntityQueryInField(ref _regionLoaderQuery)
				.ForEach((ref RegionLoader loader, in Translation t) =>
				{
					loader.regionIndex = (int2)math.floor(t.Value.xz / Constants.ChunkSize.xz);
					GetPointsInRange(loader.regionIndex, loadedPoints, loader.range);
				}).Schedule();

			return loadedPoints;
		}

		void UnloadRegions(NativeList<int2> loadedPoints)
		{
			var ecb = _endInitBarrier.CreateCommandBuffer().ToConcurrent();

			Entities
				.WithAll<UnloadRegion>()
				.ForEach((int entityInQueryIndex, Entity e, in DynamicBuffer<RegionChunksBuffer> chunks) =>
				{
					for (int i = 0; i < chunks.Length; ++i)
					{
						var chunk = chunks[i];
						if (chunk != Entity.Null)
							ecb.DestroyEntity(entityInQueryIndex, chunk);
					}
					ecb.DestroyEntity(entityInQueryIndex, e);
				}).ScheduleParallel();

			// Check that all regions are within the bounds of our region loaders
			Entities
				.WithAll<Region>()
				.WithNone<UnloadRegion>()
				.ForEach((int entityInQueryIndex, Entity e, in RegionIndex regionIndexComp,
				in DynamicBuffer<RegionChunksBuffer> chunks) =>
				{
					// Remove any currently loaded regions from the list
					int2 regionIndex = regionIndexComp;
					for (int i = loadedPoints.Length - 1; i >= 0; --i)
					{
						int2 p = loadedPoints[i];
						if (regionIndex.x == p.x && regionIndex.y == p.y)
						{
							loadedPoints.RemoveAtSwapBack(i);
							return;
						}
					}

					// Unload any regions that are outside our loader range
					ecb.AddComponent<UnloadRegion>(entityInQueryIndex, e);
					ecb.AddComponent<UpdateAdjacentChunks>(entityInQueryIndex, e);
				}).Schedule();
		}

		void LoadRegions(NativeList<int2> loadedPoints)
		{
			var regionArchetype = _regionArchetype;
			var ecb = _endInitBarrier.CreateCommandBuffer();

			ecb.RemoveComponent<LoadRegion>(_loadRegionQuery);

			Job.WithReadOnly(loadedPoints)
				.WithCode(() =>
				{
					for (int i = 0; i < loadedPoints.Length; ++i)
					{
						var region = ecb.CreateEntity(regionArchetype);
						ecb.SetComponent<RegionIndex>(region, loadedPoints[i]);
						ecb.AddComponent<UpdateAdjacentChunks>(region);
					}
				}).Schedule();
		}
	}
}
