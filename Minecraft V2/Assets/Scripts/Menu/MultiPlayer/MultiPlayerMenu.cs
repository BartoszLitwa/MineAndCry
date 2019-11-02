using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiPlayerMenu : MonoBehaviour
{
    [SerializeField] public Button RoomsListSV;

    private void Start()
    {

    }

    private void Update()
    {
        
    }

    public void JoinRoom()
    {

    }

    public void CreateRoom()
    {

    }

    public void RefreshRooms()
    {
        
    }

    public void SceneLoad(string sceneName) => SceneManager.LoadScene(sceneName);

    public void Debug(string logmsg) => UnityEngine.Debug.Log(logmsg);
}
