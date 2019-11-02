using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft V2/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Flora")]
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainHeight; //Highest point of terrain from solidground
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Flora")]
    public float majorFloraZoneScale = 1.3f;
    public int majorFloraStructure = 0;
    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f;
    public bool placeMajorFlora = true;

    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lode;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxheight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
