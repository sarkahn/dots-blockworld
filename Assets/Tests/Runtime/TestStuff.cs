using GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestStuff : MonoBehaviour
{
    public int3 Position;
    public int3 CellSize = 16;

    private void OnGUI()
    {
        GUILayout.Label($"ToLocal:{Grid3D.ToLocal(Position)}");
        GUILayout.Label($"ToLocal2:{Position & (CellSize - 1)}");
    }
}
