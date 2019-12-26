using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace BlockWorld
{
    

    public class RegionManager : JobComponentSystem
    {
        NativeHashMap<int2, Entity> _regionMap;

        JobHandle _readJobs = default;
        JobHandle _writeJobs = default;

        JobHandle FinalJobHandle;

        NativeList<int2> _regionsToLoad;

        NativeList<JobHandle> _regionReaderJobs;

        EndSimulationEntityCommandBufferSystem _ecbSystem;

        NativeList<RegionsRequest> _regionRequests;
        
        public NativeArray<Entity> GetOrLoadRegion(int2 regionIndex, Allocator allocator, ref JobHandle inputDeps)
        {
            _regionReaderJobs.Add(inputDeps);

            return default;
        }

        ///// <summary>
        ///// Returns a native array and a job handle. When the returned
        ///// jobhandle is complete the array will contain
        ///// the requested region entities.
        ///// </summary>
        ///// <param name="regionIndices"></param>
        ///// <param name="allocator"></param>
        ///// <returns></returns>
        //public (JobHandle,NativeArray<Entity>) GetOrLoadRegions(NativeList<int2> regionIndices, Allocator allocator)
        //{
        //    NativeList<Entity> regionEntities = new NativeList<Entity>(allocator);
        //    NativeList<int2> ungeneratedRegions = new NativeList<int2>(Allocator.Temp);

        //    var map = _regionEntities;
        //    var mapWriter = map.AsParallelWriter();
        //    var commandBuffer = _ecbSystem.CreateCommandBuffer().ToConcurrent();

        //    _readJobs = Job.WithCode(() =>
        //    {
        //        for (int i = 0; i < regionIndices.Length; ++i)
        //            if (!map.ContainsKey(regionIndices[i]))
        //                ungeneratedRegions.Add(regionIndices[i]);
        //    }).Schedule(_readJobs);
            
        //    //_writeJobs = Job.WithCode(() =>
        //    //{

        //    //}).Schedule(_writeJobs);

        //    //_readJobs = Job.WithCode(() =>
        //    //{

        //    //}).Schedule(_writeJobs);

        //    return (default,regionEntities.AsDeferredJobArray());
        //}

        protected override void OnCreate()
        {
            _regionMap = new NativeHashMap<int2, Entity>(100, Allocator.Persistent);
            _regionsToLoad = new NativeList<int2>(100, Allocator.Persistent);
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _regionRequests = new NativeList<RegionsRequest>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _regionMap.Dispose();
            _regionsToLoad.Dispose();
            _regionRequests.Dispose();
        }

        [RequireComponentTag(typeof(RegionsRequest))]
        [ExcludeComponent(typeof(EntityBuffer))]
        [BurstCompile]
        struct GetAllRequestedRegionsJob : IJobForEachWithEntity_EB<RegionIndexBuffer>
        {
            public NativeList<int2> indices;

            public void Execute(Entity e, int index, [ReadOnly] DynamicBuffer<RegionIndexBuffer> b0)
            {
                for(int i = 0; i < b0.Length; ++i)
                {
                    int2 regionIndex = b0[i].value;
                    if (!indices.Contains(regionIndex))
                        indices.Add(regionIndex);
                }
            }
        }
        
        

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = _ecbSystem.CreateCommandBuffer();

            var regionMap = _regionMap;

            NativeList<int2> requestedRegions = new NativeList<int2>(5, Allocator.TempJob);

            inputDeps = new GetAllRequestedRegionsJob
            {
                indices = requestedRegions
            }.ScheduleSingle(this, inputDeps);

            var deferredRequested = requestedRegions.AsDeferredJobArray();

            var uninitializedRegions = new NativeList<int2>(Allocator.TempJob);

            inputDeps = Job
                .WithReadOnly(regionMap)
                .WithCode(() =>
            {
                for( int i = 0; i < requestedRegions.Length; ++i )
                {
                    int2 regionIndex = requestedRegions[i];
                    if (!regionMap.ContainsKey(regionIndex))
                    {
                        Entity e = commandBuffer.CreateEntity();
                        var chunkBuffer = commandBuffer.AddBuffer<EntityBuffer>(e);
                    }
                }
            }).Schedule(inputDeps);

            var deferredUnitialized = uninitializedRegions.AsDeferredJobArray();

            inputDeps = Job
                .WithCode(() =>
                {
                    for( int i = 0; i < uninitializedRegions.Length; ++i )
                    {
                        int2 regionIndex = uninitializedRegions[i];

                    }
                }).Schedule(inputDeps);
            //inputDeps = Entities
            //    .WithAll<RegionsRequest>()
            //    .WithNone<EntityBuffer>()
            //    .ForEach((int entityInQueryIndex, in DynamicBuffer<RegionIndexBuffer> indexBuffer) =>
            //    {
            //        for( int i = 0; i < indexBuffer.Length; ++i )
            //        {
            //            int2 regionIndex = indexBuffer[i];

            //        }
            //    }).Schedule(inputDeps);

            //inputDeps = Entities
            //    .WithAll<RegionsRequest>()
            //    .WithNone<EntityBuffer>()
            //    .ForEach((int entityInQueryIndex, Entity e, ref DynamicBuffer<RegionIndexBuffer> indexBuffer) =>
            //    {
            //        var entityBuffer = commandBuffer.AddBuffer<EntityBuffer>(entityInQueryIndex, e);
            //        for( int i = 0; i < indexBuffer.Length; ++i )
            //        {

            //        }
            //    }).Schedule(inputDeps);

            _ecbSystem.AddJobHandleForProducer(inputDeps);

            requestedRegions.Dispose(inputDeps);
            uninitializedRegions.Dispose(inputDeps);

            return inputDeps;
        }
    }
}