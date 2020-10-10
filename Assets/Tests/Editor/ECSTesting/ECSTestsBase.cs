using System.Runtime.InteropServices;
using Unity.Entities;

// Credit to https://github.com/5argon/EcsTesting

namespace Sark.EcsTesting
{

    public abstract class ECSTestBase
    {
        protected World World { get; private set; }
        protected EntityManager EntityManager => World.EntityManager;

        protected EntityQuery CreateEntityQuery(params ComponentType[] components) => EntityManager.CreateEntityQuery(components);

        protected EntityQuery GetEntityQuery(params ComponentType[] components) =>
            CreateEntityQuery(components);

        protected void SetUpBase()
        {
            World = new World("Test World");
        }

        /// <summary>
        /// Call to make the next world update go in a specific time.
        /// </summary>
        protected void ForceDeltaTime(float deltaTime)
        {
            World.GetExistingSystem<ConstantDeltaTimeSystem>().ForceDeltaTime(deltaTime);
        }

        protected void TearDownBase()
        {
            World.Dispose();
        }
    }
}