using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    static World world;
    static Player player;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private Button SaveBtn;
    [SerializeField] private Button SaveAndExitBtn;

    static string path = "World path";
    Thread SaveWorldThread;

    private void Start()
    {
        Debug.Log("SaveManager Start method");
        path = Application.dataPath + "/Worlds/"+ Helpers.CurrentWorldname + "/";
        Debug.Log("SaveManager Path" + path);
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();

        SaveBlocktypesToFile();
        LoadBlocktypesToGame();

        PausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            world.inPauseScreen = !world.inPauseScreen;
    }

    public static void LoadPlacedBlocksFromFile()
    {
        Debug.Log("Start LoadPlacedBlocksFromFile");
        if(!File.Exists(path + "WorldSave.txt"))
        {
            Debug.Log("File doesnt exist: " + path + "WorldSave.txt");
            return;
        }

        string[] Input = File.ReadAllLines(path + "WorldSave.txt");
        foreach (string s in Input)
        {
            if(!string.IsNullOrEmpty(s))
                player.PlayersBlocksPlaced.Add(JsonUtility.FromJson<BlocksToSave>(s));
        }

        foreach (BlocksToSave b in player.PlayersBlocksPlaced)
        {
            Chunk thisChunk = world.getChunkFromVector3(b.pos);
            thisChunk.EditVoxel(b.pos, b.id);

            if (!world.chunksToUpdate.Contains(thisChunk))
            {
                world.chunksToUpdate.Add(thisChunk);
            }
        }
        Debug.Log("End LoadPlacedBlocksFromFile");
    }

    public static void GetWorldSettingsFromFile()
    {
        Debug.Log("GetWorldSettingsFromFile");
        string InputSettings = File.ReadAllText(path + "WorldSettings.txt");
        string[] Set = InputSettings.Split(" || ".ToCharArray());
        Debug.Log(InputSettings);

        int GameMode = 0;
        int.TryParse(Set[1], out GameMode);
        player.GameMode = (VoxelData.GameModes)GameMode;

        int Seed = 0;
        int.TryParse(Set[0], out Seed);
        Helpers.CurrentSeed = Seed;

        bool AllowCheats = false;
        bool.TryParse(Set[3], out AllowCheats);
        player.AllowCheats = AllowCheats;
    }

    public void SaveWorldToFile()
    {
        SaveWorldThread = new Thread(new ThreadStart(ThreadedSave));
        SaveWorldThread.Start();
    }

    private void OnDisable()
    {
        if(SaveWorldThread != null && SaveWorldThread.IsAlive)
            SaveWorldThread.Abort();
    }

    void ThreadedSave()
    {
        Debug.Log("SaveWorldToFile Start method");
        Debug.Log(player.PlayersBlocksPlaced.Count + 1);
        string[] voxels = new string[player.PlayersBlocksPlaced.Count + 1];
        //voxels[0] = JsonUtility.ToJson(Helpers.Vector3ToVector3Int(world.player.position));
        int i = 0;
        foreach (BlocksToSave b in player.PlayersBlocksPlaced)
        {
            string t = JsonUtility.ToJson(player.PlayersBlocksPlaced[i], false);
            if (string.IsNullOrEmpty(t))
                continue;

            voxels[i] = t;
            i++;
        }
        File.WriteAllLines(path + "WorldSave.txt", voxels);
        Debug.Log("SaveWorldToFile End method");
    }

    public void BackToGameBtn() => world.inPauseScreen = !world.inPauseScreen;

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    void SaveBlocktypesToFile()
    {
        string Blocks = "";
        for (int i = 0; i < world.blocktypes.Length; i++)
        {
            Blocks += JsonUtility.ToJson(world.blocktypes[i], true);
            if (i != world.blocktypes.Length - 1) //Checks if it isnt the last one
                Blocks += "||";
        }

        File.WriteAllText(path + "Blocks.txt", Blocks);
        Debug.Log("SaveBlocktypesToFile method");
    }

    void LoadBlocktypesToGame()
    {
        string loadstring = File.ReadAllText(path + "Blocks.txt");
        string[] blockstring = loadstring.Split("||".ToCharArray());
        for (int i = 1; i < world.blocktypes.Length; i = i + 2) //It reads the correct one and then the object of it so we have to add 2 every loop
        {
            JsonUtility.FromJsonOverwrite(blockstring[i], world.blocktypes[i]);
        }
        Debug.Log("LoadBlocktypesToGame method");
    }
}
