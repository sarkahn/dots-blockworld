using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LoadChunksTestCursor : MonoBehaviour
{

#if UNITY_EDITOR

    int3 _pos = 0;

    private void OnEnable()
    {

    }

    void OnSceneGUI()
    {

    }

    private void OnDrawGizmos()
    {


        //SceneView.RepaintAll();
    }


    private void OnGUI()
    {
        GUILayout.Label($"Position {_pos}");
    }
#endif

}
