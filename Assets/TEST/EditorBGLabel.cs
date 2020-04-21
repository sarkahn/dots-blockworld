using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using UnityEditor;

public class EditorBGLabel
{
    public FontStyle _fontStyle;
    GUIStyle _style;
    Color _textColor;
    Color _bgColor;
    string _content;

    GUIStyle Style
    {
        get
        {
            if (_style == null)
            {
                _style = new GUIStyle(EditorStyles.label);
                _style.fontStyle = _fontStyle;
                _style.normal.textColor = _textColor;
            }

            return _style;
        }
    }
    float2 _rectSize;

    public EditorBGLabel(string content, float2 rectSize, int fontSize = 16, Color textColor = default, Color bgColor = default, FontStyle fontStyle = FontStyle.Normal)
    {
        _content = content;
        _fontStyle = fontStyle;
        _rectSize = rectSize;
        _textColor = textColor == default ? Color.black : textColor;
        _bgColor = bgColor == default ? Color.white : bgColor;
    }

    public void Draw()
    {
        Handles.BeginGUI();

        var area = EditorGUILayout.GetControlRect(GUILayout.Width(_rectSize.x), GUILayout.Height(_rectSize.y));

        EditorGUI.DrawRect(area, _bgColor);
        EditorGUI.LabelField(area, _content, Style);

        Handles.EndGUI();
    }
}
