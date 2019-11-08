using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldItem : MonoBehaviour
{
    [SerializeField] private Text WorldText = null;
    [SerializeField] private Text LastplayedText = null;
    [SerializeField] private Text GamemodeText = null;
    [SerializeField] private Button ButtonWorld = null;

    public string WorldName;
    public string LastPlayed;
    public int GameMode;

    public void ButtonClicked()
    {
        Debug.Log("World " + WorldName + " clicked!");
    }

    public WorldItem(string _WorldName, string _LastPlayed, int _GameMode)
    {
        WorldName = _WorldName;
        LastPlayed = _LastPlayed;
        GameMode = _GameMode;
    }

    public void InitNames()
    {
        WorldText.text = WorldName;
        LastplayedText.text = LastPlayed;
        GamemodeText.text = ((VoxelData.GameModes)GameMode).ToString();
    }
}
