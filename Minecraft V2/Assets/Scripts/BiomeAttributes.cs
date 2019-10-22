using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft V2/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    public int solidGroundHeight; //Below this is alwyas solid ground
    public int terrainHeight; //Highest point of terrain from solidground
    public float terrainScale;

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
