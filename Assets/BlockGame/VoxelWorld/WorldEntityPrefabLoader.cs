using BlockGame.Chunks;
using BlockGame.Regions;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class WorldEntityPrefabLoader
{
    public const string RegionPrefabPath = "Prefabs/Region";
    public const string VoxelChunkPrefabPath = "Prefabs/VoxelChunk";

    Entity _regionPrefab = default;
    Entity _chunkPrefab = default;

    EntityManager EntityManager;

    public WorldEntityPrefabLoader(EntityManager em)
    {
        EntityManager = em;
    }

    public Entity RegionPrefab
    {
        get => GetPrefab<Region>(ref _regionPrefab, RegionPrefabPath);
    }

    public Entity VoxelChunkPrefab
    {
        get => GetPrefab<VoxelChunk>(ref _chunkPrefab, VoxelChunkPrefabPath);
    }

    Entity GetPrefab<T>(ref Entity e, string path) where T : IComponentData
    {
        if (e == Entity.Null)
            e = LoadEntity<T>(path);
        return e;
    }

    Entity LoadEntity<T>(string path) where T : IComponentData
    {
        var asset = Resources.Load<GameObject>(path);

        if (asset == null)
            throw new System.ArgumentException($"Error loading '{typeof(T).Name}' at '{path}'");

        var settings = new GameObjectConversionSettings(EntityManager.World, GameObjectConversionUtility.ConversionFlags.AssignName);
        GameObjectConversionUtility.ConvertGameObjectHierarchy(asset, settings);

        var q = EntityManager.CreateEntityQuery(typeof(T), typeof(Prefab));
        return q.GetSingletonEntity();
    }
}
