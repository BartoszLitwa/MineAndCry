using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiPlayerMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject panelCreateRoom = null;
    [SerializeField] private InputField inputRoomName = null;
    [SerializeField] private Button buttonCreateRoom = null;

    [SerializeField] private GameObject panelJoinRoom = null;
    [SerializeField] private InputField inputJoinRoomName = null;
    [SerializeField] private Button buttonJoinRoom = null;

    [SerializeField] private GameObject UIRoomsConatiner = null;
    [SerializeField] private GameObject RoomPrefab;

    private bool isConnecting = false;

    private const string GameVersion = "0.1";
    private const int MaxPlayersPerRoom = 2;

    private List<GameObject> roomListing = new List<GameObject>();

    private void Start()
    {
        panelCreateRoom.SetActive(false);

        PhotonNetwork.GameVersion = GameVersion;
        PhotonNetwork.ConnectUsingSettings();

        RefreshRoomsBtn();
    }

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

    private void Update()
    {
        
    }

    public void JoinRoom()
    {
        if(!PhotonNetwork.IsConnected) { return; }

        PhotonNetwork.JoinRoom(inputJoinRoomName.text);
    }

    public void JoinRoomBtn()
    {
        panelJoinRoom.SetActive(true);
        SetJoinRoomBtnInteractable();
    }

    public void CreateRoomBtn()
    {
        panelCreateRoom.SetActive(true);
        SetCreateRoomBtnInteractable();
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected) { return; }

        PhotonNetwork.CreateRoom(inputRoomName.text);
    }

    public void SetCreateRoomBtnInteractable() => buttonCreateRoom.interactable = !string.IsNullOrEmpty(inputRoomName.text);

    public void SetJoinRoomBtnInteractable() => buttonJoinRoom.interactable = !string.IsNullOrEmpty(inputJoinRoomName.text);

    public void RefreshRoomsBtn()
    {
        
    }

    public void SceneLoad(string sceneName) => SceneManager.LoadScene(sceneName);

    public void Debug(string logmsg) => UnityEngine.Debug.Log(logmsg);

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int index = 0;
        Vector3 pos = UIRoomsConatiner.transform.position;
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                int inList = roomListing.FindIndex(x => x.GetComponent<RoomItem>().room == room.Name); //Searches for the room with the same index
                if (inList != -1)
                {
                    //Destroy(roomListing[inList]);
                    roomListing.RemoveAt(inList);
                }
            }
            else
            {
                GameObject Room = Instantiate(RoomPrefab, new Vector3(pos.x, pos.y + 4860 - index * 105, pos.z), Quaternion.identity, UIRoomsConatiner.transform);
                if (room == null)
                    continue;

                Room.name = "Room " + room.Name;
                RoomItem item = Room.GetComponent<RoomItem>();
                item.room = room.Name;
                item.Hostname = room.masterClientId.ToString();
                item.curPlayers = room.PlayerCount;
                item.MaxPlayers = room.MaxPlayers;
                item.InitNames();

                roomListing.Add(Room);
            }

            index++;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug("Connected To master");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug("Joined the Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug($"Disconnected due to: {cause}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug("Failed to join the room " + message);
    }

    public override void OnJoinedRoom()
    {
        Debug($"{PlayerNameInput.PlayerPrefNick} has joined the room");

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if(playerCount == MaxPlayersPerRoom)
        {
            Debug("Match is ready to begin");
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            Debug("Match is ready to begin!");

            PhotonNetwork.LoadLevel("MainGame MultiPlayer");
        }
    }
}
