using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "BlockAssets", order = 10)]
public class BlockAsset : ScriptableObject
{
    [SerializeField]
    public Sprite[] _faceSprites = new Sprite[6];
}
