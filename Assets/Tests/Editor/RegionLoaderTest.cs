using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;
using Unity.Transforms;
using Unity.Entities;
using Sark.EcsTesting;
using UnityEngine.SocialPlatforms;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using BlockGame.Regions;

namespace BlockGame.RegionLoaderTests
{
    [TestFixture]
    public class RegionLoaderTest : WorldTestBase
    {
        int _chunkSize = 16;

        Entity MakeLoader(int range = 0)
        {
            var e = EntityManager.CreateEntity(
                typeof(RegionLoader), typeof(Translation));
            SetLoaderRange(e, range);
            return e;
        }

        void SetLoaderRange(Entity loader, int range)
        {
            EntityManager.SetComponentData<RegionLoader>(loader,
                new RegionLoader { Range = range });
        }

        void SetLoaderPosition(Entity loader, float3 p)
        {
            EntityManager.SetComponentData<Translation>(loader,
                new Translation { Value = p });
        }

        [Test]
        public void SingleRegionCreatesFromZeroRangeLoader()
        {
            MakeLoader();

            World.Update();

            var q = GetEntityQuery(typeof(Region));

            Assert.AreEqual(1, q.CalculateEntityCount());
        }

        [Test]
        public void LoaderDoesntRecreateAlreadyLoadedRegions()
        {
            MakeLoader();

            World.Update();
            World.Update();

            var q = GetEntityQuery(typeof(Region));

            Assert.AreEqual(1, q.CalculateEntityCount());
        }

        [Test]
        public void AreaRegionsCreateFromRangedLoader()
        {
            var loader = MakeLoader(1);

            World.Update();

            var q = GetEntityQuery(typeof(Region));

            Assert.AreEqual(9, q.CalculateEntityCount());

            SetLoaderRange(loader, 2);

            World.Update();

            Assert.AreEqual(25, q.CalculateEntityCount());
        }

        [Test]
        public void LoaderSetsPositionForGeneratedRegion()
        {
            var loader = MakeLoader();
            float3 pos = new float3(18, 25, 90);
            SetLoaderPosition(loader, pos);
            pos.y = 0;

            World.Update();

            var q = GetEntityQuery(typeof(Region));
            var region = q.GetSingletonEntity();

            var regionIndex = EntityManager.GetComponentData<Region>(region).Index;

            int2 expectedRegionIndex = (int2)(math.floor(pos.xz / _chunkSize));
            Assert.AreEqual(expectedRegionIndex, regionIndex);
        }
    }
}