using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePlayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelDeleteWorld = null;
    [SerializeField] private GameObject panelCreateWorld = null;

    [SerializeField] private Button LoadWorldBtn = null;
    [SerializeField] private Button CreateWorldBtn = null;
    [SerializeField] private Button DeleteWorldBtn = null;

    [SerializeField] private Dropdown GameModeDropdown = null;
    [SerializeField] private InputField CreateWorldNameInput = null;
    [SerializeField] private InputField WorldSeedInput = null;
    [SerializeField] private Toggle AllowCheatsToggle = null;

    [SerializeField] private GameObject UIWorldsConatiner = null;
    [SerializeField] private GameObject WorldPrefab = null;

    List<GameObject> WorldItemsList = new List<GameObject>();

    string path = "Worlds Folder";

    void Start()
    {
        path = Application.dataPath + "/Worlds";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        panelDeleteWorld.SetActive(false);
        panelCreateWorld.SetActive(false);

        CreateWorldNameInput.text = "New World";
        WorldSeedInput.text = "0";
        CreateWorldBtn.interactable = true;

        RefreshWorldlist();
    }

    void Update()
    {
        
    }

    public void CreateWorld()
    {
        Directory.CreateDirectory(path + "/" + CreateWorldNameInput.text);
        string json = JsonUtility.ToJson(getWorldParams());
        File.WriteAllText(path + "/" + CreateWorldNameInput.text + "/WorldSettings.txt", json);

        Helpers.CurrentWorldname = CreateWorldNameInput.text;
        Helpers.ThisWorld1stLoad = true;
        //RefreshWorldlist();

        SceneLoad("MainGame");
    }

    WorldSettings getWorldParams()
    {
        WorldSettings world = new WorldSettings();
        world.Seed = WorldSeedInput.text;
        world.Gamemode = GameModeDropdown.value;
        world.WorldName = CreateWorldNameInput.text;
        world.AllowCheats = AllowCheatsToggle.isOn;
        world.lastPlayed = DateTime.Now.ToString();
        return world;
    }

    public void RefreshWorldlist()
    {
        foreach (GameObject g in WorldItemsList)
        {
            GameObject.Destroy(g);
        }

        string[] WorldsFolders = Directory.GetDirectories(path).Select(Path.GetFileName).ToArray();

        int i = 0;
        Vector3 pos = UIWorldsConatiner.transform.position;
        foreach (string s in WorldsFolders)
        {
            GameObject WorldI = Instantiate(WorldPrefab, new Vector3(pos.x, pos.y + 5000 - i * 105, pos.z), Quaternion.identity, UIWorldsConatiner.transform);
            if (WorldI == null)
                continue;

            WorldI.name = "World " + s;
            WorldItem item = WorldI.GetComponent<WorldItem>();
            WorldSettings set = SaveManager.getWorldSettingsFromFile(path + "/" + s + "/WorldSettings.txt");
            Debug(set.ToString());
            item.WorldName = set.WorldName;
            item.LastPlayed = set.lastPlayed;
            item.GameMode = set.Gamemode;

            item.InitNames();

            WorldItemsList.Add(WorldI);

            Debug(s);
            i++;
        }
    }

    public void MakeCreateBtnInteractable() => CreateWorldBtn.interactable = !string.IsNullOrEmpty(CreateWorldNameInput.text);

    public void SceneLoad(string sceneName) => SceneManager.LoadScene(sceneName);

    public void Debug(string logmsg) => UnityEngine.Debug.Log(logmsg);
}
