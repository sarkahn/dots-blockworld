using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct HeightMapGenerationSettings : IComponentData
{
    public int Iterations;
    public float Persistence;
    public float Scale;
    public int Low;
    public int High;

    public static HeightMapGenerationSettings Default =>
        new HeightMapGenerationSettings
        {
            Iterations = 16,
            Persistence = .5f,
            Scale = 0.006f,
            Low = 0,
            High = 50
        };
}

public class HeightMapGenerationSettingsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    static HeightMapGenerationSettings _default = HeightMapGenerationSettings.Default;

    public int   Iterations = _default.Iterations;
    public float Persistence = _default.Persistence;
    public float Scale = _default.Scale;
    public int   Low = _default.Low;
    public int   High = _default.High;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HeightMapGenerationSettings
        {
            Iterations = Iterations,
            Persistence = Persistence,
            Scale = Scale,
            Low = Low,
            High = High,
        });
    }
}
