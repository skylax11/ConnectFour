using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString32Bytes ClientName;
    public bool IsReady;
    public int SelectedTeam;

    public bool Equals(LobbyPlayerData other)
    {
        return  ClientName.Equals(other.ClientName) && 
                ClientId == other.ClientId && 
                IsReady == other.IsReady &&
                SelectedTeam == other.SelectedTeam;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref ClientName);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref SelectedTeam);
    }
}

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public NetworkList<LobbyPlayerData> Players = new NetworkList<LobbyPlayerData>();


    private void Awake()
    {
        Instance = this;
        Players = new NetworkList<LobbyPlayerData>();
    }
    public override void OnNetworkSpawn()
    {
        Players.OnListChanged += OnPlayerListChanged;

        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        Players.OnListChanged -= OnPlayerListChanged;

        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<LobbyPlayerData> e)
    {

        bool shouldRefreshClientList = false;

        if (e.Type == NetworkListEvent<LobbyPlayerData>.EventType.Add   ||
            e.Type == NetworkListEvent<LobbyPlayerData>.EventType.Remove)
        {
            shouldRefreshClientList = true;
        }

        if (e.Type == NetworkListEvent<LobbyPlayerData>.EventType.Value)
        {
            if (e.Value.IsReady != e.PreviousValue.IsReady)
            {
                shouldRefreshClientList = true;
            }
        }

        if (shouldRefreshClientList)
        {
            LobbyClientUI.Instance.RefreshClient(Players);
        }
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }

        bool success = RemovePlayer(clientId);

        if (success)
        {
            Debug.Log("Successfully deleted user.");
        }
        else
        {
            Debug.LogWarning("No user found.");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerJoinedServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        foreach (var p in Players)
        {
            if (p.ClientId == clientId)
            {
                return;
            }
        }

        Players.Add(new LobbyPlayerData { ClientName = name + Players.Count, ClientId = clientId, SelectedTeam = Players.Count % 2 });
    }
    [ServerRpc (RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (Players.Count == 0)
        {
            return;
        }

        foreach (var p in Players)
        {
            if (!p.IsReady)
            {
                return;
            }
        }

        foreach (var p in Players)
        {
            if (NetworkManager.ConnectedClients.TryGetValue(p.ClientId, out NetworkClient client) && client.PlayerObject != null)
            {
                PlayerObject playerObject = client.PlayerObject.GetComponent<PlayerObject>();

                if (playerObject != null)
                {
                    playerObject.Team.Value = p.SelectedTeam;
                }
            }
        }

        NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
    [ServerRpc (RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        for(int i = 0; i < Players.Count; i++)
        {
            LobbyPlayerData p = Players[i];

            if (p.ClientId == clientId)
            {
                p.IsReady = !p.IsReady;
                Players[i] = p;
                return;
            }
        }

    }
    private bool RemovePlayer(ulong clientId)
    {
        for(int i = 0; i < Players.Count; i++)
        {
            LobbyPlayerData player = Players[i];

            if (player.ClientId == clientId)
            {
                Players.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public int GetClientTeam(ulong clientId)
    {
        foreach (var p in Players)
        {
            if (p.ClientId == clientId)
            {
                return p.SelectedTeam;
            }
        }

        return -1;
    }
}
