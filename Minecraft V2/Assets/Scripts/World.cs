using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int Seed;
    public BiomeAttributes Biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Blocktype[] blocktypes;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCord> ActiveChunks = new List<ChunkCord>();
    ChunkCord playerChunkCoord;
    ChunkCord playerLastCoord;

    private void Start()
    {
        Random.InitState(Seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 3, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        player.position = spawnPosition;
        playerLastCoord = GetChunkCoordFromvector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromvector3(player.position);

        if(!playerChunkCoord.Equals(playerLastCoord))
            CheckViewDistance();

        playerLastCoord = GetChunkCoordFromvector3(player.position);

        DisableInActiveChunks();
    }

    void GenerateWorld()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
        spawnPosition = new Vector3(VoxelData.WorldSizeInBlocks / 2, VoxelData.ChunkHeight / 2 + 15, VoxelData.WorldSizeInBlocks / 2);
        player.position = spawnPosition;
    }

    void DisableInActiveChunks()
    {
        foreach(ChunkCord c in ActiveChunks)
        {
            if (!chunks[c.x, c.z].IsActive)
                chunks[c.x, c.z].IsActive = false;
        }
    }

    ChunkCord GetChunkCoordFromvector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCord coord = GetChunkCoordFromvector3(player.position);

        List<ChunkCord> prevActivChunks = new List<ChunkCord>(ActiveChunks);

        for(int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if(IsChunkInWorld(new ChunkCord(x,z)))
                {
                    if (chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                        Debug.Log("Chunk generated" + coord.x + "," + coord.z);
                    }
                    else if(!chunks[x,z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                        ActiveChunks.Add(new ChunkCord(x, z));
                    }
                }

                for(int i = 0; i < prevActivChunks.Count; i++)
                {
                    if (prevActivChunks[i].Equals(new ChunkCord(x, z)))
                        prevActivChunks.RemoveAt(i);
                }
            }
        }

        foreach(ChunkCord c in prevActivChunks)
        {
            chunks[c.x, c.z].IsActive = false;
        }
    }

    public byte GetVoxel(Vector3 pos)
    {
        //Immutable Pass
        if (!IsVoxelInWorld(pos)) //Out of World
            return 0;

        int yPos = Mathf.FloorToInt(pos.y);

        if (yPos == 0) //Bottom of the chunk
            return (byte)VoxelData.BlockTypes.Bedrock;

        //Get2DPerlin return values from 0 to 1 so i need to * it by chunkheight to get real heght
        int TerrainHeight = Mathf.FloorToInt(PerlinNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, Biome.terrainScale) * Biome.terrainHeight) + Biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == TerrainHeight)
            voxelValue = (byte)VoxelData.BlockTypes.Grass;
        else if (yPos < TerrainHeight && yPos > TerrainHeight - 4)
            voxelValue = (byte)VoxelData.BlockTypes.Dirt;
        else if (yPos <= TerrainHeight - 4)
            voxelValue = (byte)VoxelData.BlockTypes.Stone;
        else
            return (byte)VoxelData.BlockTypes.Air;

        //Second Pass
        if (voxelValue == (byte)VoxelData.BlockTypes.Stone) //To generate some type of caves in stone
        {
            foreach(Lode lode in Biome.lode)
            {
                if (yPos >= lode.minHeight && yPos <= lode.maxheight)
                    if (PerlinNoise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        return voxelValue;
    }

    void CreateNewChunk(int _x, int _z)
    {
        chunks[_x, _z] = new Chunk(new ChunkCord(_x, _z), this);
        ActiveChunks.Add(new ChunkCord(_x, _z));
    }

    bool IsChunkInWorld(ChunkCord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.WorldSizeInVoxels && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }
}

[System.Serializable]
public class Blocktype
{
    public string BlockName;
    public bool IsSolid;

    [Header("textures Values")]
    public int backFacetexture;
    public int frontFacetexture;
    public int topFacetexture;
    public int bottomFacetexture;
    public int leftFacetexture;
    public int rightFacetexture;

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
