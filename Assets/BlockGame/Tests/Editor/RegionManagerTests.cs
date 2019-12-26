using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockWorld.EditorTests
{
    [TestFixture]
    public class RegionManagerTests : ECSTestsFixture
    {
        struct TestRequestTag : IComponentData { }
        

        [Test]
        public void LoadSingleRegion()
        {
            var em = m_Manager;
            

            var regionManager = World.GetOrCreateSystem<RegionManager>();

            var e = em.CreateEntity();
            var requestBuffer = em.AddBuffer<RegionIndexBuffer>(e);
            requestBuffer.Add(new int2(0, 0));
            em.AddComponent<TestRequestTag>(e);
            em.AddComponent<RegionsRequest>(e);

            regionManager.Update();

            //var resultQuery = GetEntityQuery(
            //    //ComponentType.RegionRequest
            //    );


            //var (handle, regionEntity) = regionManager.GetOrLoadRegion(new int2(0, 0), Allocator.TempJob);

            
        }

    }
}