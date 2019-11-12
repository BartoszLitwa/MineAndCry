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

    public static Vector3Int Vector3ToVector3Int(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    public static void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
}
