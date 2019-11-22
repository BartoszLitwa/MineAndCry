using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public Settings settings = new Settings();
    public string WorldName = "World";
    public Player playerClass;
    
    public BiomeAttributes[] Biomes;

    public Transform player;
    public Vector3 spawnPosition;

    [Range(0f, 1f)]
    public float globalLightLevel;
    public float DayCycle = 960f; //1 - 1s
    public Color day;
    public Color night;

    public Material material;
    public Material transparentMaterial;
    public Blocktype[] blocktypes;
    public GameObject debugScreen;
    public Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCord> ActiveChunks = new List<ChunkCord>();
    public ChunkCord playerChunkCoord;
    ChunkCord playerLastCoord;

    List<ChunkCord> chunksToCreate = new List<ChunkCord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();

    bool ApplyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    public Queue<Chunk> ChunksToDraw = new Queue<Chunk>();

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();
    public Sprite blockIconSprites;
    string path = "";

    public GameObject LoadingScreenPanel;
    public Text LoadingScreenText;
    public GameObject SettingsPauseScreenPanel;

    public bool WorldLoaded = false;
    public float timer = 0;
    public bool Day = true;

    public WorldSettings worldSettings;

    private void Start()
    {
        LoadingScreenText.text = "Creating World...";
        LoadingScreenPanel.SetActive(true);

        Debug.Log("World Start Method");
        path = Application.dataPath + "/Worlds/" + Helpers.CurrentWorldname + "/";
        playerClass = GameObject.Find("Player").GetComponent<Player>();

        ReadGameSettingsFromFile();

        worldSettings = SaveManager.getWorldSettingsFromFile(path + "WorldSettings.txt");
        playerClass.AllowCheats = worldSettings.AllowCheats;
        playerClass.GameMode = (VoxelData.GameModes)worldSettings.Gamemode;

        UnityEngine.Random.InitState(int.Parse(worldSettings.Seed));

        Camera.main.fieldOfView = settings.PlayersFOV;

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightlevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start(); //Start new Thread
        }

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastCoord = GetChunkCoordFromvector3(player.position);
    }
   
    private void Update()
    {
        HandleDayCycle();

        playerChunkCoord = GetChunkCoordFromvector3(player.position);

        if (!playerChunkCoord.Equals(playerLastCoord))
        {
            CheckViewDistance();
        }

        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }

        if (!WorldLoaded)
        {
            if (chunksToCreate.Count == 0)
            {
                WorldLoaded = true;
                LoadingScreenText.text = "Loading World...";
                if (Helpers.DoesThisWorldNeedLoad)
                {
                    SaveManager.LoadPlacedBlocksFromFile();
                    SaveManager.LoadPlayerInventoryFromFile();
                    Helpers.DoesThisWorldNeedLoad = false;
                }

                TurnOffTheLoadingScreen();
            }
        }

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

        if (Helpers.NeedReReadSettingsToGame)
        {
            ReadGameSettingsFromFile();

            Camera.main.fieldOfView = settings.PlayersFOV;

            CheckViewDistance();

            Helpers.NeedReReadSettingsToGame = false;
        }
    }

    void HandleDayCycle()
    {
        if (timer > 1) //After 5 sec change the shader
        {
            SetGlobalLightValue(globalLightLevel);
            timer = 0;
        }
        if (globalLightLevel < 0)
            Day = false;
        if (globalLightLevel >= 1)
            Day = true;

        float part = 1 / DayCycle * Time.fixedDeltaTime;
        if (Day)
        {
            globalLightLevel -= part;
        }
        else
        {
            globalLightLevel += part;
        }

        timer += Time.fixedDeltaTime;
    }

    void ReadGameSettingsFromFile()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + "/Settings.txt");
        settings = JsonUtility.FromJson<Settings>(jsonImport);
    }

    private void TurnOffTheLoadingScreen()
    {
        LoadingScreenPanel.SetActive(false);
    }

    public void SetGlobalLightValue(float globalLight)
    {
        Shader.SetGlobalFloat("GlobalLightlevel", globalLight);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLight);
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
        bool updated = false;

        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();

                    if(!ActiveChunks.Contains(chunksToUpdate[index].coord))
                        ActiveChunks.Add(chunksToUpdate[index].coord);

                    chunksToUpdate.RemoveAt(index);

                    updated = true;
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

                    if (IsChunkInViewDistance(c))
                    {
                        if (chunks[c.x, c.z] == null)
                        {
                            chunks[c.x, c.z] = new Chunk(c, this);
                            chunksToCreate.Add(c);
                        }

                        chunks[c.x, c.z].modifications.Enqueue(v); //Adds modifications to new chunk
                    }
                }
            }
        }

        ApplyingModifications = false;
    }

    bool IsChunkInViewDistance(ChunkCord coord)
    {
        if (coord.x >= playerChunkCoord.x - settings.viewDistance - 1 && coord.x <= playerChunkCoord.x + settings.viewDistance - 1 &&
            coord.z >= playerChunkCoord.z - settings.viewDistance - 1 && coord.z <= playerChunkCoord.z + settings.viewDistance - 1)
            return true;
        else
            return false;
    }

    ChunkCord GetChunkCoordFromvector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCord(x, z);
    }

    public Chunk getChunkFromVector3(Vector3 pos)
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

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable && chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos) != null)
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
            if (PerlinNoise.Get2DPerlin(Pos2, biome.offset, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (PerlinNoise.Get2DPerlin(Pos2, biome.offset, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
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