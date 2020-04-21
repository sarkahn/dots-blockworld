using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public static class EditorInput
{
    public static float2 MousePos => Event.current.mousePosition;
    public static Ray MouseRay => HandleUtility.GUIPointToWorldRay(MousePos);

    public static bool MouseButtonPressedThisFrame(int button)
    {
        if (Event.current.type != EventType.MouseDown)
            return false;

        return Event.current.button == button;
    }

    public static bool MouseIsInWindow(EditorWindow window) => EditorWindow.mouseOverWindow == window;

    public static bool KeyPressedThisFrame(KeyCode keyCode)
    {
        if (Event.current.type != EventType.KeyDown)
            return false;

        return Event.current.keyCode == keyCode;
    }
}
