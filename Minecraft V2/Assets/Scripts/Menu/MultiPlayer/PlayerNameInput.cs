using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private GameObject panelInputName = null;
    [SerializeField] private InputField inputName = null;
    [SerializeField] private Button continueButton = null;

    public const string PlayerPrefNick = "Player";

    void Start() => SetUpInputField();

    private void SetUpInputField()
    {
        continueButton.interactable = !string.IsNullOrEmpty(inputName.text);

        panelInputName.SetActive(true);

        if (!PlayerPrefs.HasKey(PlayerPrefNick)) { return; }

        string defaultName = PlayerPrefs.GetString(PlayerPrefNick);
        inputName.text = defaultName;
    }

    public void SetContinueBtninteractable()
    {
        continueButton.interactable = !string.IsNullOrEmpty(inputName.text);
    }

    public void SavePlayerName()
    {
        string name = inputName.text;

        PhotonNetwork.NickName = name;

        PlayerPrefs.SetString(PlayerPrefNick, name);

        panelInputName.SetActive(false);
    }
}
