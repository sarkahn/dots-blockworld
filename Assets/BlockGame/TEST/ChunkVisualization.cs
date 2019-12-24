using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using UnityEngine.InputSystem;
using BlockWorld;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChunkVisualization : MonoBehaviour
{
    const int chunkSizeX = 32;
    const int chunkSizeY = 64;
    const int chunkSizeZ = 32;

    [Header("BlockIndex")]
    [SerializeField]
    [Range(0, chunkSizeX - 1)]
    int cursorPosX = 0;

    [SerializeField]
    [Range(0, chunkSizeY - 1)]
    int cursorPosY = 0;

    [SerializeField]
    [Range(0, chunkSizeZ - 1)]
    int cursorPosZ = 0;

    int3 CursorPos => new int3(cursorPosX, cursorPosY, cursorPosZ);

    [Header("Chunk Index")]
    [SerializeField]
    [Range(0, 6)]
    int chunkIndexX = 0;

    [SerializeField]
    [Range(0, 6)]
    int chunkIndexY = 0;

    [SerializeField]
    [Range(0, 6)]
    int chunkIndexZ = 0;

    public int3 ChunkIndex => new int3(chunkIndexX, chunkIndexY, chunkIndexZ);
    public int3 ChunkSize => new int3(chunkSizeX, chunkSizeY, chunkSizeZ);

    float3 GetCursorPos(float3 chunkOrigin) => chunkOrigin + CursorPos;



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float3 chunkOrigin = GridMath.Grid3D.CellWorldOrigin(ChunkIndex, ChunkSize);
        float3 chunkSize = new float3(ChunkSize);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(chunkOrigin + chunkSize * .5f, chunkSize * 1f);

        float3 cursorPos = GetCursorPos(chunkOrigin);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube( cursorPos + new float3(0.5f), Vector3.one);
    }
#endif

#if UNITY_EDITOR
    private void OnGUI()
    {
        
    }
#endif
}
