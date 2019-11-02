using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomsList : MonoBehaviour
{
    [SerializeField] private GameObject UIRoomsConatiner = null;
    [SerializeField] private GameObject RoomPrefab;

    void Start()
    {
        Vector3 pos = UIRoomsConatiner.transform.position;
        for (int i = 1; i < 20; i++)
        {
            GameObject Room = Instantiate(RoomPrefab, new Vector3(pos.x, pos.y + 1100 - i * 105, pos.z), Quaternion.identity, UIRoomsConatiner.transform);
            Room.name = "Room " + i;
            RoomItem item = RoomPrefab.GetComponent<RoomItem>();
            item.room = i;
            item.Hostname = "CRNYY";
            item.curPlayers = i;
            item.MaxPlayers = 10;
            item.InitNames();
        }
    }

    void Update()
    {
        
    }
}
