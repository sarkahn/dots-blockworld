using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UIElements;

using UnityEditor;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Entities;
using BlockGame.BlockWorld;
using Unity.Collections;

public class LoadChunksEditorWindow : EditorWindow
{
    ToggleButton _editModeButton;
    const string EditModeButtonPrefsName = "LoadChunksEditorPrefs-EditMode";

    EditorBGLabel _editModeLabel;
    LoadChunksCursor _cursor;

    [MenuItem("BlockGame/LoadChunksTest")]
    public static void ShowWindow()
    {
        var window = GetWindow<LoadChunksEditorWindow>();
        window.titleContent = new GUIContent("LoadChunks");
        window.minSize = new Vector2(450, 300);
    }

    private void OnEnable()
    {
        var root = rootVisualElement;

        _editModeButton = new ToggleButton { text = "Toggle Edit Mode" };
        _editModeButton.style.height = 25;
        _editModeButton.style.fontSize = 16;
        _editModeButton.value = EditorPrefs.GetBool(EditModeButtonPrefsName, false);

        Toggle toggle = new Toggle();
        root.Add(_editModeButton);

        SceneView.duringSceneGui += OnSceneGUI;

        _cursor = new LoadChunksCursor(16, Color.blue);
    }

    private void OnGUI()
    {
        SceneView.RepaintAll();
    }


    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;

        EditorPrefs.SetBool(EditModeButtonPrefsName, _editModeButton.value);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (_editModeButton.value)
        {
            // override default control
            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Selection.activeObject = null;
        }
        else 
            return;

        DrawEditModeSceneLabel();
        DrawCursor();

        HandleInput();
    }

    void DrawCursor()
    {
        var mouseRay = EditorInput.MouseRay;
        var surfacePlane = new Plane(Vector3.up, 0);

        if (!surfacePlane.Raycast(mouseRay, out float dist))
            return;
        float3 surfacePoint = mouseRay.GetPoint(dist);
        surfacePoint.y = 0;
        _cursor.WorldPos = surfacePoint;

        _cursor.Draw();
    }


    void DrawEditModeSceneLabel()
    {
        Color textColor = Color.red;
        Color bgColor = new Color(.85f, .85f, .85f);
        float width = 135;
        string content = "Edit Mode Active";
        int fontSize = 14;

        var style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = textColor;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = fontSize;
        Handles.BeginGUI();
        {
            var area = EditorGUILayout.GetControlRect(GUILayout.Width(width));

            EditorGUI.DrawRect(area, bgColor);
            if (GUI.Button(area, content, style))
            {
                _editModeButton.value = false;
            }
        }
        Handles.EndGUI();

        SceneView.RepaintAll();
    }

    void HandleInput()
    {
        if (Event.current.type == EventType.Repaint)
            return; 

        if (!EditorInput.MouseIsInWindow(SceneView.lastActiveSceneView))
            return;

        if (EditorInput.MouseButtonPressedThisFrame(0))
        {
            if (!Application.isPlaying)
                return;

            var world = World.DefaultGameObjectInjectionWorld;
            var em = world.EntityManager;

            var index = _cursor.IndexPos;

            var registry = world.GetOrCreateSystem<RegionRegistrySystem>();
            if(registry.TryGetRegion(index.xz, out Entity region) )
            {
                em.AddComponent<UnloadRegion>(region);
            }
            else
            {
                var loadEntity = em.CreateEntity(typeof(LoadRegion), typeof(RegionIndex));
                em.SetComponentData<RegionIndex>(loadEntity, _cursor.IndexPos.xz);
            }

        }
    }
}
