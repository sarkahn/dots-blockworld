using System.Collections;
using System.Collections.Generic;
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
		EndInitializationEntityCommandBufferSystem _barrier;
		EntityArchetype _regionArchetype;

		protected override void OnCreate()
		{
			base.OnCreate();

			_barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

			_regionArchetype = EntityManager.CreateArchetype(typeof(Region), typeof(GenerateRegion));
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
			GenerateRegionHeightMapSettings settings = GenerateRegionHeightMapSettings.Default;
			var settingsUI = GameObject.FindObjectOfType<MapGenerationSettingsUI>();
			if (settingsUI != null)
				settings = settingsUI.Settings;

			//var map = _regionMap;
			//var mapWriter = map.AsParallelWriter();

			var ecb = _barrier.CreateCommandBuffer().ToConcurrent();

			var regionArchetype = _regionArchetype;

			NativeList<int2> loadedPoints = new NativeList<int2>(100, Allocator.TempJob);

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
				.ForEach((int entityInQueryIndex, Entity e, in RegionIndex regionIndexComp, 
				in DynamicBuffer<LinkedEntityGroup> linkedGroup) =>
				{
					// Remove any currently loaded regions from the list
					int2 regionIndex = regionIndexComp;
					for( int i = loadedPoints.Length - 1; i >= 0; --i )
					{
						int2 p = loadedPoints[i];
						if(regionIndex.x == p.x && regionIndex.y == p.y )
						{
							loadedPoints.RemoveAtSwapBack(i);
							return;
						}
					}

					// Any regions not in our loaded list can be unloaded/destroyed
					// Note that "LinkedEntityGroup" only destroys linked entities automatically if 
					// the root entity is destroyed via an entity query, so we do it manually here.
					for (int i = 0; i < linkedGroup.Length; ++i)
						ecb.DestroyEntity(entityInQueryIndex, linkedGroup[i].Value);
					ecb.DestroyEntity(entityInQueryIndex, e);

				}).Schedule();

			// Load any remaining unloaded regions
			Dependency = new LoadUnloadedRegionsJob
			{
				genSettings = settings,
				chunkArchetype = regionArchetype,
				loadedPoints = loadedPoints.AsDeferredJobArray(),
				ecb = _barrier.CreateCommandBuffer().ToConcurrent(),
			}.Schedule(loadedPoints, 64, Dependency);

			loadedPoints.Dispose(Dependency);

			_barrier.AddJobHandleForProducer(Dependency);
		}

		struct LoadUnloadedRegionsJob : IJobParallelForDefer
		{
			[NativeSetThreadIndex]
#pragma warning disable 0649
			int m_ThreadIndex;
#pragma warning restore 0649

			[ReadOnly]
			public NativeArray<int2> loadedPoints;

			public EntityCommandBuffer.Concurrent ecb;
			public EntityArchetype chunkArchetype;
			public GenerateRegionHeightMapSettings genSettings;

			public void Execute(int index)
			{
				var regionEntity = ecb.CreateEntity(m_ThreadIndex, chunkArchetype);
				ecb.SetComponent(m_ThreadIndex, regionEntity, new GenerateRegion
				{
					regionIndex = loadedPoints[index]
				});
				ecb.AddComponent(m_ThreadIndex, regionEntity, genSettings);
			}
		}
	}
}
