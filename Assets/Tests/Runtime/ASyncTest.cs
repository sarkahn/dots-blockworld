using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Assertions;
using BlockGame.Regions;

public class ASyncTest : MonoBehaviour
{
    World _stagingWorld;
    EntityManager EntityManager => _stagingWorld.EntityManager;

    [SerializeField]
    int _amount = 100000;

    struct MakeEntitiesJob : IJob
    {
        public ExclusiveEntityTransaction Transaction;
        public int Amount;

        public void Execute()
        {
            for (int i = 0; i < Amount; ++i)
            {
                Entity e = Transaction.CreateEntity();
                Transaction.AddComponent(e, typeof(Region));
            }
        }
    }

    private void Awake()
    {
        _stagingWorld = new World("Staging World");
    }

    // Start is called before the first frame update
    async void Start()
    {
        var task = TaskFunc();
        Debug.Log("Just ran taskfunc");
        StartCoroutine(WaitForTask(task));

        await task;

        Debug.Log("TASK COMPLETE YO");
    }

    IEnumerator WaitForTask(Task task)
    {
        while (!task.IsCompleted)
            yield return null;
        Debug.Log("TASK COMPLETED");
    }

    async Task TaskFunc()
    {
        Debug.Log("Starting await");
        await Task.Delay(4000);
        Debug.Log("Starting Make Entities Job");
        var entities = await MakeEntitiesASync(_amount);
        Debug.Log($"Finished making {_amount} entities!");
        Assert.IsTrue(entities.Length == _amount);
        Assert.IsTrue(entities[0] != Entity.Null);
        Assert.IsTrue(HasComponent<Region>(entities[1]));
        entities.Dispose();
    }

    bool HasComponent<T>(Entity e) where T : IComponentData =>
        World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<T>(e);

    public void MakeEntities2()
    {
        var entities = MakeEntitiesASync(100);
    }

    public async Task<NativeArray<Entity>> MakeEntitiesASync(int amount)
    {
        var tr = EntityManager.BeginExclusiveEntityTransaction();
        var job = new MakeEntitiesJob
        {
            Transaction = tr,
            Amount = amount
        }.Schedule();

        while (!job.IsCompleted)
            await Task.Yield();

        EntityManager.EndExclusiveEntityTransaction();
        job.Complete();

        World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(
            out var arr,
            EntityManager
            );
        return arr;
    }
}
