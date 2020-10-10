using System.Collections;
using System.Collections.Generic;
using BlockGame.Chunks;
using BlockGame.Regions;
using NUnit.Framework;
using Sark.EcsTesting;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

public class TestGenerateRegion : WorldTestBase
{
    Entity MakeRegion(int x, int y)
    {
        var archetype = EntityManager.CreateArchetype(
            typeof(Region),
            typeof(GenerateRegion),
            typeof(VoxelChunkStack),
            typeof(LinkedEntityGroup));
        var region = EntityManager.CreateEntity(archetype);
        EntityManager.SetComponentData(region, new Region
        {
            Index = new int2(x,y)
        });
        var buffer = EntityManager.GetBuffer<LinkedEntityGroup>(region);
        buffer.Add(region);
        return region;
    }

    [Test]
    public void TestGenerateRegionSimplePasses()
    {
        var region = MakeRegion(0, 0);
        World.Update();
        var stack = EntityManager.GetBuffer<VoxelChunkStack>(region);
        Assert.NotZero(stack.Length);
    }

}
