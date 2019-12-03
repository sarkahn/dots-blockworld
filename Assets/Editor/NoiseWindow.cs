using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

public class NoiseWindow : EditorWindow
{
    float selectValue_ = 0;

    const int TexMinSize = 5;
    const int TexMaxSize = 300;
    const float OutputMinScaling = .0001f;
    const float OutputMaxScaling = 5f;
    const float InputMinScaling = .0001f;
    const float InputMaxScaling = 1f;

    [SerializeField]
    [Range(TexMinSize, TexMaxSize)]
    int texWidth_ = TexMinSize;

    [SerializeField]
    [Range(TexMinSize, TexMaxSize)]
    int texHeight_ = TexMinSize;

    [SerializeField]
    [Range(OutputMinScaling, OutputMaxScaling)]
    float outputScaling_ = OutputMinScaling;

    [SerializeField]
    [Range(InputMinScaling, InputMaxScaling)]
    float inputScaling_ = .01f;

    Texture2D tex_ = null;


    private void OnGUI()
    {
        bool update = false;

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawTextureSizingControls();

            if (check.changed)
            {
                update = true;
                ResizeTexture();
            }
        }

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawNoiseControls();

            if (check.changed)
            {
                update = true;
            }
        }
        
        InitializeTexture();

        if (update)
            ApplyNoise();

        Rect texArea = EditorGUILayout.GetControlRect(GUILayout.Width(texWidth_), GUILayout.Height(texHeight_));
        texArea.position += new Vector2(20, 20);


        EditorGUI.DrawPreviewTexture(texArea, tex_);

    }

    void DrawTextureSizingControls()
    {
        EditorGUILayout.LabelField("Texture Dimensions", EditorStyles.boldLabel);

        using (new EditorGUI.IndentLevelScope())
        {
            texWidth_ = EditorGUILayout.IntSlider("Width", texWidth_, TexMinSize, TexMaxSize);
            texHeight_ = EditorGUILayout.IntSlider("Height", texHeight_, TexMinSize, TexMaxSize);
        }
    }

    void DrawNoiseControls()
    {
        EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);

        using (new EditorGUI.IndentLevelScope())
        {
            selectValue_ = EditorGUILayout.Slider("Select Value", selectValue_, -1f, 1f);

            inputScaling_ = EditorGUILayout.Slider("Input Scaling", inputScaling_, InputMinScaling, InputMaxScaling);

            outputScaling_ = EditorGUILayout.Slider("Output Scaling", outputScaling_, OutputMinScaling, OutputMaxScaling);
        }
    }

    void ApplyNoise()
    {
        var colors = tex_.GetRawTextureData<Color32>();
        
        for( int x = 0; x < tex_.width; ++x )
        {
            for( int y = 0; y < tex_.height; ++y )
            {
                float2 @in = new float2((float)x * (float)inputScaling_, (float)y * (float)inputScaling_);
                float v = noise.snoise(@in);
                // convert to 0..1
                v = (v / 2f) + .5f;

                v *= outputScaling_;

                int i = y * tex_.width + x;

                colors[i] = (Color.white * v);

                //Debug.LogFormat("Index {0}, {1}: {2}", x, y, i);
            }
        }

        tex_.Apply();
    }

    void InitializeTexture()
    {
        int w = texWidth_;
        int h = texHeight_;

        if (tex_ == null)
        {
            tex_ = new Texture2D(w, h);
            FillTexture();
        }

        if (tex_.width != w || tex_.height != h)
            ResizeTexture();
    }

    void ResizeTexture()
    {
        tex_.Resize(texWidth_, texHeight_);
    }

    void FillTexture()
    {
        var colors = tex_.GetRawTextureData<Color32>();
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = Color.white;
        tex_.Apply();
    }

    [MenuItem("Window/Noise Window")]
    static void MakeWindow()
    {
        GetWindow<NoiseWindow>().Show();
    }
}
