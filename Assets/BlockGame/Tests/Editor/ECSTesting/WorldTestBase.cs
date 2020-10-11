using NUnit.Framework;
using Unity.Entities;
using System.Linq;

// Credit to https://github.com/5argon/EcsTesting

namespace Sark.EcsTesting
{
    public abstract class WorldTestBase : ECSTestBase
    {
        [SetUp]
        public void SetUp()
        {
            SetUpBase();
            SetUpWorld();
            // Unity lazily generates the world time component on the first world update. This will cause a structural change which could invalidate certain state during testing
            World.Update();
        }

        [TearDown]
        public void TearDown() => TearDownBase();

        private void SetUpWorld()
        {
            var allSystems =
                DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default, requireExecuteAlways: false);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World,
                allSystems.Concat(new[] { typeof(ConstantDeltaTimeSystem) }));
        }

        protected T AddSystem<T>() where T : SystemBase
        {
            DefaultWorldInitialization. AddSystemsToRootLevelSystemGroups(World, new[] { typeof(T) });
            return World.GetExistingSystem<T>();
        }
    }
}