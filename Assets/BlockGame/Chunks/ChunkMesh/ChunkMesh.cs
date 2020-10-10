using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

struct ChunkMeshVerts : IBufferElementData
{
    public int3 Value;
    public static implicit operator int3(ChunkMeshVerts b) => b.Value; 
    public static implicit operator ChunkMeshVerts(int3 v) => 
        new ChunkMeshVerts { Value = v };
}

struct ChunkMeshIndices : IBufferElementData
{
    public int Value;
    public static implicit operator int(ChunkMeshIndices b) => b.Value;
    public static implicit operator ChunkMeshIndices(int v) =>
        new ChunkMeshIndices { Value = v };
}

struct ChunkMeshUVs : IBufferElementData
{
    public float2 Value;
    public static implicit operator float2(ChunkMeshUVs b) => b.Value;
    public static implicit operator ChunkMeshUVs(float2 v) =>
        new ChunkMeshUVs { Value = v };
}