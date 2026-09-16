using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyClientUI : MonoBehaviour
{
    public static LobbyClientUI Instance;

    [SerializeField]
    private LobbyPlayer _playerLobbyObject;

    [SerializeField]
    private Transform _blueTeamPivot;

    [SerializeField]
    private Transform _redTeamPivot;

    [SerializeField]
    private Button _readyButton;

    [SerializeField]
    private Button _startButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _readyButton.onClick.AddListener(OnReadyClicked);
        _startButton.onClick.AddListener(OnStartClicked);

    }

    private void OnStartClicked()
    {

    }

    private void OnReadyClicked()
    {
        LobbyManager.Instance.SetPlayerReadyServerRpc();
    }

    public void RefreshClient(NetworkList<LobbyPlayerData> lobbyPlayers)
    {
        ClearAll();

        for(int i = 0; i < lobbyPlayers.Count; i++)
        {
            LobbyPlayerData player = lobbyPlayers[i];

            Transform team = i % 2 == 0 ? _blueTeamPivot : _redTeamPivot;
            LobbyPlayer p = Instantiate(_playerLobbyObject, team);

            p.Setup(player.ClientName,player.IsReady);
        }
    }

    private void ClearAll()
    {
        foreach(Transform t in _blueTeamPivot)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in _redTeamPivot)
        {
            Destroy(t.gameObject);
        }
    }
}
