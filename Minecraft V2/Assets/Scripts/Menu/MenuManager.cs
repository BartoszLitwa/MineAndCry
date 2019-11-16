using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Slider mouseSensSlider;
    public Text MouseSensText;
    public Slider viewDistanceSlider;
    public Text viewDistanceText;
    public Slider FOVSlider;
    public Text FOVText;
    public Toggle enableThreadingToggle;
    public Toggle chunkAnimToggle;

    static Settings settings;

    string SettingsPath = "/Settings.txt";
    int index = 0;
    bool CanSaveChanges = false;

    private void Start()
    {
        settings = new Settings();
        LoadSettingsToMenu();
        CanSaveChanges = true;
    }

    private void Update()
    {

    }

    public void SettingsChanged()
    {
        if (!CanSaveChanges)
            return;

        settings.mouseSensitivity = Mathf.FloorToInt(mouseSensSlider.value);
        settings.viewDistance = Mathf.FloorToInt(viewDistanceSlider.value);
        settings.PlayersFOV = Mathf.FloorToInt(FOVSlider.value);
        settings.enableChunkLoadAnimation = chunkAnimToggle.isOn;
        settings.enableThreading = enableThreadingToggle.isOn;

        ChangeTextValues();

        SaveChangesToFile();

        Helpers.NeedReReadSettingsToGame = true;
    }

    void ChangeTextValues()
    {
        MouseSensText.text = settings.mouseSensitivity.ToString();
        viewDistanceText.text = settings.viewDistance.ToString();
        FOVText.text = settings.PlayersFOV.ToString();
    }

    void SaveChangesToFile()
    {
        Debug.Log("SaveChangesToFile");
        string jsonExport = JsonUtility.ToJson(settings, true);
        File.WriteAllText(Application.dataPath + SettingsPath, jsonExport);
    }

    void LoadSettingsToMenu()
    {
        Debug.Log("LoadSettingsToMenu");
        LoadSettingsFile();

        viewDistanceSlider.value = settings.viewDistance;
        mouseSensSlider.value = settings.mouseSensitivity;
        chunkAnimToggle.isOn = settings.enableChunkLoadAnimation;
        enableThreadingToggle.isOn = settings.enableThreading;

        ChangeTextValues();
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
