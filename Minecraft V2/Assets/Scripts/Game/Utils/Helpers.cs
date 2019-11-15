using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Helpers
{
    public static string CurrentWorldname = "";
    public static bool DoesThisWorldNeedLoad = false;
    public static string LastClickedWorldItem = "";
    public static int CurrentSeed = 0;
    public static bool NeedReReadSettingsToGame = false;
    public static bool SurivivalInevntoryNeedUpdate = false;

    public static List<UIItemSlots> itemslots = new List<UIItemSlots>();
    public static ToolBar toolbar;
    public static Vector3 toolbarScaleOpenedSurivivalInevntory = new Vector3(1.85f, 1.7f, 1f);
    public static Vector3 toolbarScaleClosedSurivivalInevntory = new Vector3(1f, 1f, 1f);
    public static Vector3 toolbarPosOpenedSurivivalInevntory = new Vector3(960, 396, 0);
    public static Vector3 toolbarPosClosedSurivivalInevntory = new Vector3(960, 30, 0);

    public static Vector3Int Vector3ToVector3Int(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    public static void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
}
