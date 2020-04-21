using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(LoadChunksTestCursor))]
public class LoadChunksTestCursorInspector : Editor
{

    static Vector2 GetMousePos()
    {
        //return Mouse.current.position.ReadValue();
        return Event.current.mousePosition;
    }

    static Ray GetMouseRay(Vector2 mousePos)
    {
        //return Camera.current.ScreenPointToRay(mousePos);
        return HandleUtility.GUIPointToWorldRay(mousePos);
    }

    static float3 ? GetSurfacePoint(Ray ray)
    {
        var plane = new Plane(Vector3.up, 0);
        if (!plane.Raycast(ray, out float d))
            return null;
        return ray.GetPoint(d);
    }

    static float3 ? GetCursorPos()
    {
        return GetSurfacePoint(GetMouseRay(GetMousePos()));
    }


    private void OnSceneGUI()
    {
        var cursorPos = GetCursorPos();
        if (cursorPos == null)
            return;

        var style = EditorStyles.helpBox;

        float3 p = cursorPos.Value;
        float3 size = 1;

        Handles.DrawWireCube(p, size);

        Handles.BeginGUI();

        using (new EditorGUILayout.VerticalScope(style, GUILayout.Width(5)) )
        {
            EditorGUILayout.LabelField("hello");
        }

        Handles.EndGUI();

        SceneView.RepaintAll();
    }

    [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    public static void OnDrawGizmos(LoadChunksTestCursor tar, GizmoType gizmoType)
    {
        var cursorPos = GetCursorPos();
        if (cursorPos == null)
            return;

        float3 p = cursorPos.Value;
        float3 size = 1;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(p, size);
    }

    /*        var cam = Camera.current;

        var mousePos = Mouse.current.position.ReadValue();
        var mouseRay = //Application.isPlaying ? 
            //Camera.main.ScreenPointToRay(mousePos);
             HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);


        //if (Application.isPlaying)
        //    mouseRay = Camera.main.ScreenPointToRay(mousePos);
        //else
        //    mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        var surfacePlane = new Plane(Vector3.up, 0);

        float3 p = 0;
        float3 size = Constants.ChunkSize;

        if (surfacePlane.Raycast(mouseRay, out float dist))
        {
            p = mouseRay.GetPoint(dist);
            p.y = 0;
            p = math.floor(p / Constants.ChunkSize) * Constants.ChunkSize;
            p.y = 0;
        }
        else
            return;

        _pos = (int3)p / Constants.ChunkSize;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(p + size * .5f, size);*/
}
