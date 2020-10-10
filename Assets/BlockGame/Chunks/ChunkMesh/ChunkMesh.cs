using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(0)]
public struct ChunkMeshVerts : IBufferElementData
{
    public float3 Value;
    public static implicit operator float3(ChunkMeshVerts b) => b.Value; 
    public static implicit operator ChunkMeshVerts(float3 v) => 
        new ChunkMeshVerts { Value = v };
}

[InternalBufferCapacity(0)]
public struct ChunkMeshIndices : IBufferElementData
{
    public int Value;
    public static implicit operator int(ChunkMeshIndices b) => b.Value;
    public static implicit operator ChunkMeshIndices(int v) =>
        new ChunkMeshIndices { Value = v };
}

[InternalBufferCapacity(0)]
public struct ChunkMeshUVs : IBufferElementData
{
    public float2 Value;
    public static implicit operator float2(ChunkMeshUVs b) => b.Value;
    public static implicit operator ChunkMeshUVs(float2 v) =>
        new ChunkMeshUVs { Value = v };
}