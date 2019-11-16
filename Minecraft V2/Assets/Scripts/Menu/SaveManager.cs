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
    static Thread SaveWorldThread;

    private void Start()
    {
        Debug.Log("SaveManager Start method");
        path = Application.dataPath + "/Worlds/"+ Helpers.CurrentWorldname + "/";
        Debug.Log(path);
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();

        if(Helpers.ThisWorld1stLoad)
            SaveBlocktypesToFile();

        LoadBlocktypesToGame();

        PausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            player.inPauseScreen = !player.inPauseScreen;
    }

    public static WorldSettings getWorldSettingsFromFile(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<WorldSettings>(json);
    }

    public static void LoadPlayerInventoryFromFile()
    {
        Debug.Log("LoadPlayerInventoryToFile Start");
        string[] json = File.ReadAllLines(path + "/PlayerSlots.txt");
        int index = 0;
        foreach (string s in json)
        {
            if (s == "EmptySlot" || s == "Toolbar slots:")
            {
                index++;
                continue;
            }

            if (index < 27) //Helpers.itemslots
            {
                Helpers.itemslots[index].itemslot.Set(JsonUtility.FromJson<ItemStack>(s));
                Helpers.itemslots[index].UpdateSlot();
            }
            else //Helpers.toolbar.slots
            {
                Helpers.toolbar.slots[index - 28].itemslot.Set(JsonUtility.FromJson<ItemStack>(s));
                Helpers.toolbar.slots[index - 28].UpdateSlot();
            }
            index++;
        }
        Debug.Log("LoadPlayerInventoryToFile End");
    }

    public static void SavePlayerInventoryToFile()
    {
        Debug.Log("SavePlayerInventoryToFile");
        int index = 0;
        string[] json = new string[37];
        foreach(UIItemSlots item in Helpers.itemslots)
        {
            string t = JsonUtility.ToJson(item.itemslot.stack, false);
            if (String.IsNullOrEmpty(t))
            {
                json[index] = "EmptySlot";
            }
            else
                json[index] = t;
            index++;
        }
        json[index++] = "Toolbar slots:";
        foreach (UIItemSlots item2 in Helpers.toolbar.slots)
        {
            string t = JsonUtility.ToJson(item2.itemslot.stack, false);
            if (String.IsNullOrEmpty(t))
            {
                json[index] = "EmptySlot";
            }
            else
                json[index] = t;
            index++;
        }

        File.WriteAllLines(path + "/PlayerSlots.txt", json);
    }


    public static void LoadPlacedBlocksFromFile()
    {
        Debug.Log("Start LoadPlacedBlocksFromFile");
        if(!File.Exists(path + "WorldSave.txt"))
        {
            Debug.Log("File doesnt exist: " + path + "WorldSave.txt");
            return;
        }

        List<Chunk> chunksToUpdate = new List<Chunk>();
        string[] Input = File.ReadAllLines(path + "WorldSave.txt");
        foreach (string s in Input)
        {
            if (!string.IsNullOrEmpty(s))
            {
                BlocksToSave b = JsonUtility.FromJson<BlocksToSave>(s);
                if (b == null) continue;
                player.PlayersBlocksPlaced.Add(b);

                Chunk thisChunk = world.getChunkFromVector3(b.pos);
                if (thisChunk == null) continue;

                thisChunk.EditVoxel(b.pos, b.id);

                if (!world.chunksToUpdate.Contains(thisChunk))
                {
                    world.chunksToUpdate.Add(thisChunk);
                }
            }
        }
        Debug.Log("End LoadPlacedBlocksFromFile");
    }

    public void SaveWorldToFile()
    {
        SavePlayerInventoryToFile();

        Debug.Log("SaveWorldToFile Start method");
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

    private void OnDisable()
    {
        if(SaveWorldThread != null && SaveWorldThread.IsAlive)
            SaveWorldThread.Abort();
    }

    public void BackToGameBtn() => player.inPauseScreen = !player.inPauseScreen;

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    void SaveBlocktypesToFile()
    {
        Debug.Log("SaveBlocktypesToFile method");
        string Blocks = "";
        for (int i = 0; i < world.blocktypes.Length; i++)
        {
            Blocks += JsonUtility.ToJson(world.blocktypes[i], true);
            if (i != world.blocktypes.Length - 1) //Checks if it isnt the last one
                Blocks += "||";
        }

        File.WriteAllText(path + "Blocks.txt", Blocks);
    }

    void LoadBlocktypesToGame()
    {
        Debug.Log("LoadBlocktypesToGame method");
        string loadstring = File.ReadAllText(path + "Blocks.txt");
        string[] blockstring = loadstring.Split("||".ToCharArray());
        for (int i = 1; i < world.blocktypes.Length; i = i + 2) //It reads the correct one and then the object of it so we have to add 2 every loop
        {
            JsonUtility.FromJsonOverwrite(blockstring[i], world.blocktypes[i]);
        }
    }
}
