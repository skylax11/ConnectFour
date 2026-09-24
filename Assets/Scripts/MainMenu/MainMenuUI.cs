using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public event Action OnTimeLeaderboardRequested;
    public event Action OnWinCountLeaderboardRequested;

    [Header("Leaderboard")]

    [SerializeField]
    private GameObject _leaderboardPanel;

    [SerializeField]
    private TextMeshProUGUI _leaderboardText;

    [SerializeField]
    private Button _timeLeaderboard;

    [SerializeField]
    private Button _winCountLeaderboard;

    [SerializeField]
    private Button _leaderboardBtn;

    [SerializeField]
    private Button _closeLeaderboardBtn;

    [Header("User Related")]

    [SerializeField]
    private TextMeshProUGUI _welcomerTxt;

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

        _leaderboardBtn.onClick.AddListener(ToggleLeaderboard);
        _closeLeaderboardBtn.onClick.AddListener(CloseLeaderboard);
        _timeLeaderboard.onClick.AddListener(() => OnTimeLeaderboardRequested?.Invoke());
        _winCountLeaderboard.onClick.AddListener(() => OnWinCountLeaderboardRequested?.Invoke());
    }
    private void Start()
    {
        _welcomerTxt.text = AuthSession.Username;
        _leaderboardPanel.SetActive(false);
    }

    private void ToggleLeaderboard()
    {
        _leaderboardPanel.SetActive(!_leaderboardPanel.activeSelf);
    }

    private void CloseLeaderboard()
    {
        _leaderboardPanel.SetActive(false);
    }

    public void SetLeaderboardText(string content)
    {
        _leaderboardText.text = content;
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
