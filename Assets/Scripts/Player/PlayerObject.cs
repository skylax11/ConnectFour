using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerObject : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Name = new NetworkVariable<FixedString32Bytes>
        (writePerm:NetworkVariableWritePermission.Server);

    public NetworkVariable<int> Team = new NetworkVariable<int>
        (writePerm:NetworkVariableWritePermission.Server);


    private void Start()
    {
        if (IsOwner)
        {
            StartCoroutine(StartClient());
        }
    }
    IEnumerator StartClient()
    {
        yield return new WaitUntil( () =>
        {
            return LobbyManager.Instance != null && LobbyManager.Instance.IsSpawned;
        });

        string playerName = Bootstrap.Instance != null
        ? Bootstrap.Instance.PlayerName
        : $"Player {NetworkManager.Singleton.LocalClientId}";

        LobbyManager.Instance.OnPlayerJoinedServerRpc(playerName);
    }
}
