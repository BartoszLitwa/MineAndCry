using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [SerializeField] private Text roomText = null;
    [SerializeField] private Text DescText = null;
    [SerializeField] private Text PlayersText = null;
    [SerializeField] private Button ButtonRoom = null;

    public string room;
    public string Hostname;
    public int curPlayers;
    public int MaxPlayers = 2;

    void Start()
    {
        ButtonRoom.onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        Debug.Log("Room " + room + " clicked!");
    }

    public RoomItem(string _room, string _Hostname, int _curPlayers, int _MaxPlayers)
    {
        room = _room;
        Hostname = _Hostname;
        curPlayers = _curPlayers;
        MaxPlayers = _MaxPlayers;
    }

    public void InitNames()
    {
        roomText.text = room;
        DescText.text = "Created By " + Hostname;
        PlayersText.text = curPlayers + " / " + MaxPlayers;
    }
}
