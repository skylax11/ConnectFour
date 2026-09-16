using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Create Lobby")]

    [SerializeField]
    private GameObject _createLobbyPanel;

    [SerializeField]
    private Button _createLobbyPanelBtn;

    [SerializeField]
    private TMP_InputField _lobbyName;  

    [SerializeField]
    private Button _createLobby;

    [Header("List / Join Lobbies")]

    [SerializeField]
    private Button _listLobbies;   


    private void Awake()
    {
        _createLobbyPanelBtn.onClick.AddListener(() =>
        {
            _createLobbyPanel.SetActive(!_createLobbyPanel.activeSelf);
        });

        _createLobby.onClick.AddListener(() =>
        {
            string lobbyName = _lobbyName != null ? _lobbyName.text : string.Empty;
            Bootstrap.Instance.CreateLobby(lobbyName);
        });

        _listLobbies.onClick.AddListener(() =>
        {
            Bootstrap.Instance.JoinLobby();
        });
    }

    private void OnEnable()
    {
        if (Bootstrap.Instance == null) return;

        Bootstrap.Instance.OnConnectionFailed += HandleConnectionFailed;
    }

    private void OnDisable()
    {
        if (Bootstrap.Instance == null) return;

        Bootstrap.Instance.OnConnectionFailed -= HandleConnectionFailed;
    }

    private void HandleConnectionFailed(string reason)
    {
        Debug.LogWarning($"[MainMenu] Bağlantı başarısız: {reason}");
    }
}
