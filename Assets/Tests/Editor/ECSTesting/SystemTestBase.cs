using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Entities;

namespace Sark.EcsTesting
{ 
    public abstract class SystemTestBase<T> : ECSTestBase where T : ComponentSystemBase
    {
        [SetUp]
        public void SetUp()
        {
            SetUpBase();
            SetUpWorld();
        }

        [TearDown]
        public void TearDown() => TearDownBase();

        private void SetUpWorld()
        {
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World,
                new List<Type> { typeof(T), typeof(ConstantDeltaTimeSystem) });
        }
    }
}