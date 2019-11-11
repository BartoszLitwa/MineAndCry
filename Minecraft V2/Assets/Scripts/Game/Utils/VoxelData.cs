using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16; //Chunk size
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 30;

    //Lighting Values
    public static float minLightlevel = 0.15f;
    public static float maxLightLevel = 0.8f;
    public static float lightFallOff = 0.16f;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize {
        get{ return 1f / TextureAtlasSizeInBlocks; }
    }

    public enum BlockTypes { Air, Bedrock, Grass, Stone, Dirt, Sand, OakWood, Cobble, Glass, Brick, Planks, Leaves, Sandstone, Cactus, CraftingTable, Chest};

    public enum Biomes { GrassLands, Desert, Forest};

    public enum GameModes { Creative, Survival };

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f), //0
        new Vector3(1.0f, 0.0f, 0.0f), //1
        new Vector3(1.0f, 1.0f, 0.0f), //2
        new Vector3(0.0f, 1.0f, 0.0f), //3
        new Vector3(0.0f, 0.0f, 1.0f), //4
        new Vector3(1.0f, 0.0f, 1.0f), //5
        new Vector3(1.0f, 1.0f, 1.0f), //6
        new Vector3(0.0f, 1.0f, 1.0f), //7
    };

    public static readonly Vector3[] faceCheks = new Vector3[6] { //For checking if should render the sides of cube
         new Vector3(0.0f, 0.0f, -1.0f), //Back Face   Check
         new Vector3(0.0f, 0.0f, 1.0f),  //Front Face  Check
         new Vector3(0.0f, 1.0f, 0.0f),  //Top Face    Check
         new Vector3(0.0f, -1.0f, 0.0f), //Bottom Face Check
         new Vector3(-1.0f, 0.0f, 0.0f), //Left Face   Check
         new Vector3(1.0f, 0.0f, 0.0f)   //Right Face  Check
    };

    public static readonly int[,] voxelTris = new int[6, 4] { 
        //Back Front Top Bottom Left Right
       {0,3,1,2 }, //Back Face        
       {5,6,4,7 }, //Front Face       
       {3,7,2,6 }, //Top Face         
       {1,5,0,4 }, //Bottom Face      
       {4,7,0,3 }, //Left Face        
       {1,2,5,6 }  //Right Face       
    }; //Old pattern 0 1 2 2 1 3 It repeats

    public static readonly Vector2[] voxelUVs = new Vector2[4] {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };
}
