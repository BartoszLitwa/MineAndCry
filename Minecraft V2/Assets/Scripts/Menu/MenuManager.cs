using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Slider mouseSensSlider;
    public Slider viewDistanceSlider;
    public Toggle enableThreadingToggle;
    public Toggle chunkAnimToggle;

    Settings settings;
    string SettingsPath = "/Settings.cfg";
    int index = 0;

    private void Update()
    {
        if (index > 100)
        {
            LoadSettingsToMenu();
            index = -1;
        }
        else if(index != -1)
            index++;
    }

    public void SettingsChanged()
    {
        settings.mouseSensitivity = mouseSensSlider.value;
        settings.viewDistance = (int)viewDistanceSlider.value;
        settings.enableChunkLoadAnimation = chunkAnimToggle.isOn;
        settings.enableThreading = enableThreadingToggle.isOn;

        SaveChangesToFile();
    }

    void SaveChangesToFile()
    {
        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + SettingsPath, jsonExport);
    }

    void LoadSettingsToMenu()
    {
        LoadSettingsFile();
        viewDistanceSlider.value = settings.viewDistance;
        mouseSensSlider.value = settings.mouseSensitivity;
        
        chunkAnimToggle.isOn = settings.enableChunkLoadAnimation;
        enableThreadingToggle.isOn = settings.enableThreading;
    }

    void LoadSettingsFile()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + SettingsPath);
        settings = JsonUtility.FromJson<Settings>(jsonImport);
    }

    public void LoadSceneButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SettingsMenuButton()
    {
        SettingsChanged();
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}
