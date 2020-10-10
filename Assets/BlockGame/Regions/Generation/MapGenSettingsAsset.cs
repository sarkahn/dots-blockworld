using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "MapGenSettings", menuName = "BlockWorld/MapGen Settings")]
public class MapGenSettingsAsset : ScriptableObject
{
    public MapGenSettings Settings = MapGenSettings.Default;
}

[System.Serializable]
public struct MapGenSettings
{
    public int Iterations;
    public float Persistance;
    public float Scale;
    public int Low;
    public int High;

    public static MapGenSettings Default => new MapGenSettings
    {
        Iterations = 16,
        Persistance = .5f,
        Scale = 0.01f,
        Low = 0,
        High = 15
    };
}

