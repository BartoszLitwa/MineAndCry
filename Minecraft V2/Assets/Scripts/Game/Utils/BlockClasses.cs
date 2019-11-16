using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettings
{
    public string WorldName;
    public string Seed;
    public int Gamemode;
    public bool AllowCheats;
    public string lastPlayed;
}

public class BlocksToSave
{
    public Vector3Int pos;
    public byte id; //If id == 0 block got removed

    public BlocksToSave(Vector3Int _pos, byte _id)
    {
        pos = _pos;
        id = _id;
    }
}

[System.Serializable]
public class Blocktype
{
    public string BlockName;
    public bool IsSolid; //Is phisical block unlike air
    public bool renderNeighbourFaces; //Can see thru a block
    public bool hasGravity;
    public float transparency;
    public Sprite icon;

    [Header("textures Values")]
    public int backFacetexture;
    public int frontFacetexture;
    public int topFacetexture;
    public int bottomFacetexture;
    public int leftFacetexture;
    public int rightFacetexture;

    public Blocktype(Blocktype b)
    {
        BlockName = b.BlockName;
        IsSolid = b.IsSolid;
        renderNeighbourFaces = b.renderNeighbourFaces;
        hasGravity = b.hasGravity;
        transparency = b.transparency;
        //icon = b.icon;
        backFacetexture = b.backFacetexture;
        frontFacetexture = b.frontFacetexture;
        topFacetexture = b.topFacetexture;
        bottomFacetexture = b.bottomFacetexture;
        leftFacetexture = b.leftFacetexture;
        rightFacetexture = b.rightFacetexture;
    }

    //Back Front Top Bottom Left Right
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFacetexture;
            case 1:
                return frontFacetexture;
            case 2:
                return topFacetexture;
            case 3:
                return bottomFacetexture;
            case 4:
                return leftFacetexture;
            case 5:
                return rightFacetexture;
            default:
                Debug.Log("Error in GetTextureID; Invalid face Index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte ID;

    public VoxelMod(Vector3 _pos, byte _id)
    {
        position = _pos;
        ID = _id;
    }

    public VoxelMod()
    {
        position = new Vector3();
        ID = 0;
    }
}

public class VoxelState
{
    public byte id;
    public float globalLightPercent;
    //public VoxelData.Biomes Biome;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f; //Dark block
        //Biome = VoxelData.Biomes.GrassLands;
    }

    public VoxelState(byte _id)
    {
        id = _id;
        globalLightPercent = 0f; //Dark block
    }

    public VoxelState(byte _id, VoxelData.Biomes _biome)
    {
        id = _id;
        globalLightPercent = 0f; //Dark block
        //Biome = _biome;
    }
}