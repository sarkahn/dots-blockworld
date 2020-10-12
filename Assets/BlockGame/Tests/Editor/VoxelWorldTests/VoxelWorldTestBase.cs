using NUnit.Framework;
using Sark.BlockGame;
using Sark.EcsTesting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelWorldTestBase : WorldTestBase
{
    protected VoxelWorldSystem System;
    protected VoxelWorld VoxelWorld => System.GetVoxelWorld();

    [SetUp]
    public void Setup()
    {
        System = AddSystem<VoxelWorldSystem>();
    }
}
