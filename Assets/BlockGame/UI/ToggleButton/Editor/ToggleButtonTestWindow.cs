using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class ToggleButtonWindow : EditorWindow
{
    [MenuItem("Window/ToggleButtonTest")]
    public static void ShowWindow() => GetWindow<ToggleButtonWindow>().titleContent = new GUIContent("ToggleButton Test");

    private void OnEnable()
    {
        var root = rootVisualElement;

        var toggleButton = new ToggleButton() { text = "Toggle Button" };

        root.Add(toggleButton);

        var button = new Button() { text = "Comparison Button" };

        root.Add(button);
    }
}
