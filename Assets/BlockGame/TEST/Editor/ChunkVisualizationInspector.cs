using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using BlockWorld;

[CustomEditor(typeof(ChunkVisualization))]
public class ChunkVisualizationInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Cursor Info", EditorStyles.boldLabel);

        int cursorX = serializedObject.FindProperty("cursorPosX").intValue;
        int cursorY = serializedObject.FindProperty("cursorPosY").intValue;
        int cursorZ = serializedObject.FindProperty("cursorPosZ").intValue;
        int3 cursorXYZ = new int3(cursorX, cursorY, cursorZ);

        int chunkX = serializedObject.FindProperty("chunkIndexX").intValue;
        int chunkY = serializedObject.FindProperty("chunkIndexX").intValue;
        int chunkZ = serializedObject.FindProperty("chunkIndexX").intValue;
        int3 chunkIndex = new int3(chunkX, chunkY, chunkZ);

        var chunkSize = (target as ChunkVisualization).ChunkSize;

        int worldIndex = GridMath.Grid3D.ArrayIndexFromCellPos(cursorXYZ + chunkIndex * chunkSize, chunkSize);

        EditorGUILayout.LabelField($"CursorPos: {cursorXYZ}");
        EditorGUILayout.LabelField($"WorldIndex: {worldIndex}");

    }
}
