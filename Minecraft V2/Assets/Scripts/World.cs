using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;

public class World : MonoBehaviour
{
    public Settings settings;

    public BiomeAttributes[] Biomes;

    public Transform player;
    public Vector3 spawnPosition;

    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Material material;
    public Material transparentMaterial;
    public Blocktype[] blocktypes;
    public GameObject debugScreen;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCord> ActiveChunks = new List<ChunkCord>();
    public ChunkCord playerChunkCoord;
    ChunkCord playerLastCoord;

    List<ChunkCord> chunksToCreate = new List<ChunkCord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();

    bool ApplyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    public Queue<Chunk> ChunksToDraw = new Queue<Chunk>();

    private bool _inUI = false;
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    private void Start()
    {
        //string jsonExport = JsonUtility.ToJson(settings);
        //File.WriteAllText(Application.dataPath + "/Settings.cfg", jsonExport);

        string jsonImport = File.ReadAllText(Application.dataPath + "/Settings.cfg");
        settings = JsonUtility.FromJson<Settings>(jsonImport);

        Random.InitState(settings.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightlevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start(); //Start new Thread
        }

        SetGlobalLightValue();

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        player.position = spawnPosition;
        playerLastCoord = GetChunkCoordFromvector3(player.position);

    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromvector3(player.position);

        if (!playerChunkCoord.Equals(playerLastCoord))
        {
            CheckViewDistance();
            DisableInActiveChunks();
        }

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (!settings.enableThreading)
        {
            if (!ApplyingModifications)
                ApllyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }

        if (ChunksToDraw.Count > 0)
        {
            if (ChunksToDraw.Peek().isEditable)
                ChunksToDraw.Dequeue().CreateMesh(); //remove and run createmesh
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightlevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    void GenerateWorld()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; z++)
            {
                ChunkCord newChunk = new ChunkCord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);
            }
        }

        player.position = spawnPosition;

        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        int index = 0;

        lock (ChunkUpdateThreadLock)
        {
            while (index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();

                    if(!ActiveChunks.Contains(chunksToUpdate[index].coord))
                        ActiveChunks.Add(chunksToUpdate[index].coord);

                    chunksToUpdate.RemoveAt(index);
                }
                else
                    index++;
            }
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!ApplyingModifications)
                ApllyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    void ApllyModifications()
    {
        ApplyingModifications = true;

        while(modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            if (queue != null)
            {
                while (queue.Count > 0)
                {
                    VoxelMod v = queue.Dequeue();
                    ChunkCord c = GetChunkCoordFromvector3(v.position);

                    if (chunks[c.x, c.z] == null)
                    {
                        chunks[c.x, c.z] = new Chunk(c, this);
                        chunksToCreate.Add(c);
                    }

                    chunks[c.x, c.z].modifications.Enqueue(v);
                }
            }
        }

        ApplyingModifications = false;
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

    public Chunk getChunkFromvector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return chunks[x, z];
    }

    void CheckViewDistance()
    {
        ChunkCord coord = GetChunkCoordFromvector3(player.position);
        playerLastCoord = playerChunkCoord;

        List<ChunkCord> prevActivChunks = new List<ChunkCord>(ActiveChunks);

        ActiveChunks.Clear(); //After copying clear the list

        for(int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
            {
                ChunkCord thischunkCoord = new ChunkCord(x, z);
                if (IsChunkInWorld(thischunkCoord))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(thischunkCoord, this);
                        chunksToCreate.Add(thischunkCoord);
                    }
                    else if(!chunks[x,z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                    }
                    ActiveChunks.Add(thischunkCoord);
                }

                for(int i = 0; i < prevActivChunks.Count; i++)
                {
                    if (prevActivChunks[i].Equals(thischunkCoord))
                        prevActivChunks.RemoveAt(i);
                }
            }
        }

        foreach(ChunkCord c in prevActivChunks)
        {
            chunks[c.x, c.z].IsActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCord thisChunk = new ChunkCord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blocktypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].IsSolid;

        return blocktypes[GetVoxel(pos)].IsSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCord thisChunk = new ChunkCord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return null;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);

        return new VoxelState(GetVoxel(pos));
    }

    public bool inUI
    {
        get {  return _inUI; }
        set {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }

    public int solidGroundHeight = 42;

    public byte GetVoxel(Vector3 pos)
    {
        //Immutable Pass
        if (!IsVoxelInWorld(pos)) //Out of World
            return 0;

        int yPos = Mathf.FloorToInt(pos.y);

        if (yPos == 0) //Bottom of the chunk
            return (byte)VoxelData.BlockTypes.Bedrock;

        //Biome Selection Pass
        float sumOffheights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestWeightIndex = 0;
        Vector2 Pos2 = new Vector2(pos.x, pos.z);

        for (int b = 0; b < Biomes.Length; b++)
        {
            float weight = PerlinNoise.Get2DPerlin(Pos2, Biomes[b].offset, Biomes[b].scale);

            if(weight > strongestWeight) //Sets the highest wieght
            {
                strongestWeight = weight;
                strongestWeightIndex = b;
            }

            //get the height of current biome
            float height = Biomes[b].terrainHeight * PerlinNoise.Get2DPerlin(Pos2, 0, Biomes[b].terrainScale) * weight;

            if(height > 0)
            {
                sumOffheights += height;
                count++;
            }
        }

        BiomeAttributes biome = Biomes[strongestWeightIndex];
        sumOffheights /= count;

        int TerrainHeight = Mathf.FloorToInt(sumOffheights + solidGroundHeight);

        //Get2DPerlin return values from 0 to 1 so i need to * it by chunkheight to get real heght
        byte voxelValue = 0;

        if (yPos == TerrainHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < TerrainHeight && yPos > TerrainHeight - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos <= TerrainHeight - 4)
            voxelValue = (byte)VoxelData.BlockTypes.Stone;
        else
            return (byte)VoxelData.BlockTypes.Air;

        //Second Pass
        if (voxelValue == (byte)VoxelData.BlockTypes.Stone) //To generate some type of caves in stone
        {
            foreach(Lode lode in biome.lode)
            {
                if (yPos >= lode.minHeight && yPos <= lode.maxheight)
                    if (PerlinNoise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        //Flora Pass
        if(yPos == TerrainHeight && biome.placeMajorFlora)
        {
            if (PerlinNoise.Get2DPerlin(Pos2, 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (PerlinNoise.Get2DPerlin(Pos2, 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora((Structure.StructureType)biome.majorFloraStructure, pos, biome.minHeight, biome.maxHeight));
                    voxelValue = biome.surfaceBlock;
                }
            }
        }

        return voxelValue;
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
    public bool IsSolid; //Is phisical block unlike air
    public bool renderNeighbourFaces; //Can see thru a block
    public float transparency;
    public Sprite icon;

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

[System.Serializable] //It needs to be serializable to see it in unity
public class Settings
{
    [Header("Game Data")]
    public string version;

    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;

    [Header("World Generation")]
    public int seed;
    public int WorldSizeInChunks;
    public bool enableChunkLoadAnimation;

    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity;
}
