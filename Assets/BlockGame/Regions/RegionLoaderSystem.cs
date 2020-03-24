using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
		NativeHashMap<int2, Entity> _regionMap;

		EndInitializationEntityCommandBufferSystem _barrier;
		EntityArchetype _regionArchetype;

		EntityQuery _loaderQuery;

		EntityQuery _regionQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			// Yeah, it's gross
			_regionMap = new NativeHashMap<int2, Entity>(100000, Allocator.Persistent);

			_barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

			_regionArchetype = EntityManager.CreateArchetype(typeof(Region), typeof(GenerateRegion));

			_loaderQuery = EntityManager.CreateEntityQuery(
				ComponentType.ReadOnly<RegionLoader>()
				);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_regionMap.Dispose(Dependency);
		}

		static void GetPointsInRange(int2 start, NativeList<int2> points, int range)
		{
			for (int x = -range; x < range; ++x)
				for (int z = -range; z < range; ++z)
				{
					int2 p = start + new int2(x, z);
					if (GridUtil.Grid2D.TaxicabDistance(start, p) > range)
						continue;
					if (!points.Contains(p))
						points.Add(p);
				}
		}

		protected override void OnUpdate()
		{
			GenerateRegionHeightMapSettings settings = GenerateRegionHeightMapSettings.Default;
			var settingsUI = GameObject.FindObjectOfType<MapGenerationSettingsUI>();
			if (settingsUI != null)
				settings = settingsUI.Settings;

			var map = _regionMap;
			var mapWriter = map.AsParallelWriter();

			var ecb = _barrier.CreateCommandBuffer().ToConcurrent();

			var regionArchetype = _regionArchetype;

			Job.WithCode(() =>
			{
				map.Clear();
			}).Schedule();

			NativeList<int2> loadedPoints = new NativeList<int2>(50, Allocator.TempJob);

			int range = GameObject.FindObjectOfType<MapGenerationSettingsUI>()._range;

			// Assign region loader indices
			Entities
				.ForEach((ref RegionLoader loader, in Translation t) =>
			{
				loader.regionIndex = (int2)math.floor(t.Value.xz / Constants.ChunkSize.xz);
				GetPointsInRange(loader.regionIndex, loadedPoints, range);
			}).Schedule();

			Entities
				.WithAll<Region>()
				.ForEach((int entityInQueryIndex, Entity e, in RegionIndex regionIndex, 
				in DynamicBuffer<LinkedEntityGroup> linkedGroup) =>
				{
					int2 ind = regionIndex;
					for( int i = loadedPoints.Length - 1; i >= 0; --i )
					{
						int2 p = loadedPoints[i];
						if(ind.x == p.x && ind.y == p.y )
						{
							loadedPoints.RemoveAtSwapBack(i);
							return;
						}
					}

					for (int i = 0; i < linkedGroup.Length; ++i)
						ecb.DestroyEntity(entityInQueryIndex, linkedGroup[i].Value);
					ecb.DestroyEntity(entityInQueryIndex, e);
					//int i = loadedPoints.IndexOf(regionIndex.value);
					//if(i < 0)
					//{
					//	//ecb.DestroyEntity(entityInQueryIndex, e);
					//}
					//else
					//{
					//	loadedPoints.RemoveAtSwapBack(i);
					//}
				}).Schedule();

			var buffer = _barrier.CreateCommandBuffer();

			Job
				.WithoutBurst()
				.WithReadOnly(loadedPoints).WithCode(() =>
			{
				for( int i = 0; i < loadedPoints.Length; ++i )
				{
					//Debug.Log($"Adding region at {loadedPoints[i]}");
					var regionEntity = buffer.CreateEntity(regionArchetype);
					buffer.SetComponent(regionEntity,
						new GenerateRegion
						{
							regionIndex = loadedPoints[i]
						});
					buffer.AddComponent<GenerateRegionHeightMapSettings>(regionEntity, settings);
				}
			}).Schedule();

			loadedPoints.Dispose(Dependency);


			//// Dependency errors...
			////var loaders = _loaderQuery.ToComponentDataArrayAsync<RegionLoader>(Allocator.TempJob, out var loaderJob);
			////Dependency = JobHandle.CombineDependencies(Dependency, loaderJob);
			////var loaders = _loaderQuery.ToComponentDataArray<RegionLoader>(Allocator.TempJob);
			//NativeList<RegionLoader> loadersList = new NativeList<RegionLoader>(10, Allocator.TempJob);
			//Entities
			//	.ForEach((in RegionLoader loader) =>
			//	{
			//		loadersList.Add(loader);
			//	}).Schedule();

			//var loaders = loadersList.AsDeferredJobArray();


			//// Add existing regions to the map
			//Entities
			//	.WithoutBurst()
			//	.WithReadOnly(loaders)
			//	.WithNone<GenerateRegion>()
			//	.ForEach((Entity e, in RegionIndex regionIndexComp) =>
			//	{
			//		int2 regionIndex = regionIndexComp;
			//		for ( int i = 0; i < loaders.Length; ++i )
			//		{
			//			int2 loaderIndex = loaders[i].regionIndex;
			//			int dist = math.abs(loaderIndex.x - regionIndex.x) + math.abs(loaderIndex.y - regionIndex.y);
			//			if( dist <= loaders[i].range )
			//				mapWriter.TryAdd(regionIndexComp, e);
			//		}
			//	}).ScheduleParallel();

			//loadersList.Dispose(Dependency);

			// Unload regions
			//Entities
			//	.WithReadOnly(map)
			//	.ForEach((int entityInQueryIndex, Entity e, in RegionIndex regionIndexComp) =>
			//	{
			//		//if (!map.ContainsKey(regionIndexComp))
			//		//	commandBuffer.DestroyEntity(entityInQueryIndex, e);
			//	}).ScheduleParallel();

			// Generate any regions within our range that aren't already generated
			//Entities
			//	.WithReadOnly(map)
			//	.ForEach((int entityInQueryIndex, Entity e, in RegionLoader loader) =>
			//	{
			//		int range = loader.range;
			//		if (range <= 0)
			//			return;

			//		int2 regionPos = loader.regionIndex;

			//		for (int x = -range; x < range; ++x)
			//			for (int z = -range; z < range; ++z)
			//			{
			//				int2 p = regionPos + new int2(x, z);

			//				if (!map.ContainsKey(p))
			//				{
			//					var regionEntity = commandBuffer.CreateEntity(entityInQueryIndex, regionArchetype);
			//					commandBuffer.SetComponent(entityInQueryIndex, regionEntity, 
			//						new GenerateRegion
			//						{
			//							regionIndex = p
			//						});

			//					commandBuffer.AddComponent(entityInQueryIndex, regionEntity, loader.heightMapSettings);
			//				}
			//			}
			//	}).ScheduleParallel();

			_barrier.AddJobHandleForProducer(Dependency);
		}
	}
}
