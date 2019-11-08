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

    [SerializeField] private Button LoadWorld = null;
    [SerializeField] private Button CreateWorld = null;
    [SerializeField] private Button DeleteWorld = null;

    [SerializeField] private Dropdown GameMode = null;
    [SerializeField] private InputField CreateWorldName = null;
    [SerializeField] private Toggle AllowCheatsToggle = null;

    [SerializeField] private GameObject UIWorldsConatiner = null;
    [SerializeField] private GameObject WorldPrefab = null;

    string path = "Worlds Folder";

    void Start()
    {
        path = Application.dataPath + "/Worlds";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        panelDeleteWorld.SetActive(false);
        panelCreateWorld.SetActive(false);

        RefreshWorldlist();
    }

    void Update()
    {
        
    }

    void RefreshWorldlist()
    {
        string[] WorldsFolders = Directory.GetDirectories(path).Select(Path.GetFileName).ToArray();

        int i = 0;
        Vector3 pos = UIWorldsConatiner.transform.position;
        foreach (string s in WorldsFolders)
        {
            GameObject WorldI = Instantiate(WorldPrefab, new Vector3(pos.x, pos.y + 4860 - i * 105, pos.z), Quaternion.identity, UIWorldsConatiner.transform);
            if (WorldI == null)
                continue;

            WorldI.name = "World " + i;
            WorldItem item = WorldI.GetComponent<WorldItem>();
            item.WorldName = s;
            item.LastPlayed = DateTime.Now.Date.ToString();
            item.GameMode = i;

            item.InitNames();

            Debug(s);
            i++;
        }
    }

    public void SceneLoad(string sceneName) => SceneManager.LoadScene(sceneName);

    public void Debug(string logmsg) => UnityEngine.Debug.Log(logmsg);
}
