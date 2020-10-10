using Unity.Core;
using Unity.Entities;

//Credit to https://github.com/5argon/EcsTesting

namespace Sark.EcsTesting
{
    /// <summary>
    /// This is for test only. It disables the regular one and can hack any delta time instead.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(UpdateWorldTimeSystem))]
    [DisableAutoCreation]
    public class ConstantDeltaTimeSystem : ComponentSystem
    {
        float defaultDeltaTime = 1 / 60f;
        private float simulatedElapsedTime;
        private float constantDeltaTime;

        protected override void OnCreate()
        {
            //Only works if Unity's time system is there already, this would
            //try to replace it. You should not add this system before Unity's.
            var timeSystem = World.GetExistingSystem<UpdateWorldTimeSystem>();
            if (timeSystem != null)
            {
                timeSystem.Enabled = false;
            }
            this.constantDeltaTime = defaultDeltaTime;
        }

        public void ForceDeltaTime(float dt)
        {
            this.constantDeltaTime = dt;
        }

        public void RestoreDeltaTime() => this.constantDeltaTime = defaultDeltaTime;

        protected override void OnUpdate()
        {
            simulatedElapsedTime += constantDeltaTime;
            World.SetTime(new TimeData(
                elapsedTime: simulatedElapsedTime,
                deltaTime: constantDeltaTime
            ));
        }
    }
}