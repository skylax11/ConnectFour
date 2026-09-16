using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Persistent networking bootstrap for MatchFour.
/// NGO (Netcode for GameObjects) direct/IP connection — no cloud services.
///
/// MainMenu kullanımı:
///     _createLobby.onClick.AddListener(() =>
///     {
///         Bootstrap.Instance.CreateLobby(_lobbyName.text);
///     });
///
/// Başka bir cihazdan katılmak için:
///     Bootstrap.Instance.JoinLobby("192.168.1.42");   // host'un IP'si
/// </summary>
public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }

    [Header("Connection")]
    [SerializeField] private string _listenAddress = "0.0.0.0";  // host tüm arayüzleri dinler
    [SerializeField] private string _defaultConnectAddress = "127.0.0.1";
    [SerializeField] private ushort _port = 7777;

    [Header("Scenes")]
    [SerializeField] private string _lobbySceneName = "LobbyScene";

    /// <summary>Şu an aktif lobinin ismi (host tarafında set edilir).</summary>
    public string CurrentLobbyName { get; private set; }

    /// <summary>Bu oyuncunun lobide görünecek ismi. MainMenu'de set edilebilir.</summary>
    public string PlayerName = "Player";

    /// <summary>true = host olarak lobi kurduk, false = client olarak bağlandık.</summary>
    public bool IsHost { get; private set; }

    // Event'ler — MainMenu / UI bunları dinleyip ekran değiştirebilir.
    public event Action OnLobbyCreated;                 // host başarıyla başladı (transport seviyesi)
    public event Action OnConnected;                    // client sunucuya bağlandı (transport seviyesi)
    public event Action<string> OnConnectionFailed;     // bağlantı hatası (mesaj)
    public event Action OnDisconnected;

    /// <summary>
    /// LobbyScene senkronize olup yerel oyuncu tamamen lobiye girdiğinde tetiklenir.
    /// LobbyManager bu noktada hazırdır — request'i burada at.
    /// Hem host hem client için çalışır.
    /// </summary>
    public event Action OnJoinedLobby;

    private NetworkManager _netManager;
    private bool _joinedLobbyInvoked;
    private bool _sceneEventsHooked;

    private void Start()
    {
        // Singleton — sahneler arası korunur.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _netManager = NetworkManager.Singleton;
        if (_netManager == null)
        {
            Debug.LogError("[Bootstrap] Sahnede NetworkManager (Singleton) bulunamadı. " +
                           "NGO NetworkManager objesini sahneye ekle.");
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    /// <summary>
    /// Lobiyi host olarak kurar. MainMenu'den lobi ismini geçir.
    /// </summary>
    public bool CreateLobby(string lobbyName)
    {
        if (!EnsureReady()) return false;

        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            Debug.LogWarning("[Bootstrap] Lobi ismi boş, varsayılan isim atanıyor.");
            lobbyName = $"Lobby-{UnityEngine.Random.Range(1000, 9999)}";
        }

        CurrentLobbyName = lobbyName.Trim();

        // Transport ayarları — host olarak dinle.
        var transport = _netManager.GetComponent<UnityTransport>();
        transport.SetConnectionData(_listenAddress, _port, _listenAddress);

        // Lobi ismini bağlantı payload'ı olarak client'lara iletmek için sakla.
        _netManager.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(CurrentLobbyName);

        bool started = _netManager.StartHost();
        if (started)
        {
            IsHost = true;
            _joinedLobbyInvoked = false;
            Debug.Log($"[Bootstrap] Lobi kuruldu: \"{CurrentLobbyName}\" @ {_listenAddress}:{_port}");
            OnLobbyCreated?.Invoke();

            // Sahne senkron event'lerini bağla, sonra LobbyScene'i yükle.
            HookSceneEvents();
            LoadLobbyScene();
        }
        else
        {
            Debug.LogError("[Bootstrap] StartHost() başarısız oldu.");
            OnConnectionFailed?.Invoke("Host başlatılamadı.");
        }

        return started;
    }

    /// <summary>
    /// Var olan bir lobiye client olarak bağlanır. Host'un IP adresini geçir.
    /// (null/boş bırakırsan varsayılan adres kullanılır — aynı makinede test için.)
    /// </summary>
    public bool JoinLobby(string hostAddress = null)
    {
        if (!EnsureReady()) return false;

        string address = string.IsNullOrWhiteSpace(hostAddress) ? _defaultConnectAddress : hostAddress.Trim();

        var transport = _netManager.GetComponent<UnityTransport>();
        transport.SetConnectionData(address, _port);

        bool started = _netManager.StartClient();
        if (started)
        {
            IsHost = false;
            _joinedLobbyInvoked = false;
            HookSceneEvents();
            Debug.Log($"[Bootstrap] Lobiye bağlanılıyor: {address}:{_port}");
        }
        else
        {
            Debug.LogError("[Bootstrap] StartClient() başarısız oldu.");
            OnConnectionFailed?.Invoke("Bağlantı başlatılamadı.");
        }

        return started;
    }

    /// <summary>Lobiden çıkar / bağlantıyı keser.</summary>
    public void LeaveLobby()
    {
        UnhookSceneEvents();

        if (_netManager != null && _netManager.IsListening)
        {
            _netManager.Shutdown();
            Debug.Log("[Bootstrap] Lobiden çıkıldı.");
        }

        CurrentLobbyName = null;
        IsHost = false;
        _joinedLobbyInvoked = false;
        OnDisconnected?.Invoke();
    }

    // -------- Lobiye giriş (sahne senkron) yakalama --------

    private void HookSceneEvents()
    {
        if (_sceneEventsHooked || _netManager?.SceneManager == null) return;

        _netManager.SceneManager.OnLoadEventCompleted += HandleLoadEventCompleted;
        _netManager.SceneManager.OnSynchronizeComplete += HandleSynchronizeComplete;

        _sceneEventsHooked = true;
    }

    private void UnhookSceneEvents()
    {
        if (!_sceneEventsHooked || _netManager?.SceneManager == null) return;

        _netManager.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompleted;
        _netManager.SceneManager.OnSynchronizeComplete -= HandleSynchronizeComplete;

        _sceneEventsHooked = false;
    }

    // Host tarafında LobbyScene tamamen yüklenince.
    private void HandleLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode,
        System.Collections.Generic.List<ulong> clientsCompleted,
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (!IsHost) return;
        if (sceneName != _lobbySceneName) return;

        RaiseJoinedLobby();
    }

    // Client tarafında sunucuyla senkron bitince (LobbyScene yüklü + objeler spawn edilmiş).
    private void HandleSynchronizeComplete(ulong clientId)
    {
        if (IsHost) return;
        if (clientId != _netManager.LocalClientId) return;

        RaiseJoinedLobby();
    }

    private void RaiseJoinedLobby()
    {
        if (_joinedLobbyInvoked) return;
        _joinedLobbyInvoked = true;

        Debug.Log("[Bootstrap] Lobiye girildi — LobbyManager hazır, request atılabilir.");
        OnJoinedLobby?.Invoke();
    }

    /// <summary>
    /// LobbyScene'i NGO ağ sahne yöneticisiyle yükler (client'lar da takip eder).
    /// Sahne Build Settings'e ekli ve NetworkConfig.EnableSceneManagement açık olmalı.
    /// </summary>
    private void LoadLobbyScene()
    {
        if (string.IsNullOrWhiteSpace(_lobbySceneName))
        {
            Debug.LogWarning("[Bootstrap] Lobby scene ismi boş — sahne yüklenmedi.");
            return;
        }

        if (!_netManager.NetworkConfig.EnableSceneManagement)
        {
            Debug.LogWarning("[Bootstrap] EnableSceneManagement kapalı. Sahne local yükleniyor.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(_lobbySceneName);
            return;
        }

        var status = _netManager.SceneManager.LoadScene(
            _lobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);

        if (status != SceneEventProgressStatus.Started)
            Debug.LogError($"[Bootstrap] LobbyScene yüklenemedi: {status}");
    }

    private bool EnsureReady()
    {
        if (_netManager == null)
            _netManager = NetworkManager.Singleton;

        if (_netManager == null)
        {
            Debug.LogError("[Bootstrap] NetworkManager yok — işlem iptal.");
            OnConnectionFailed?.Invoke("NetworkManager bulunamadı.");
            return false;
        }

        if (_netManager.GetComponent<UnityTransport>() == null)
        {
            Debug.LogError("[Bootstrap] NetworkManager'da UnityTransport bileşeni yok.");
            OnConnectionFailed?.Invoke("UnityTransport bulunamadı.");
            return false;
        }

        if (_netManager.IsListening)
        {
            Debug.LogWarning("[Bootstrap] Zaten aktif bir oturum var. Önce LeaveLobby() çağır.");
            return false;
        }

        return true;
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Client tarafında kendi bağlantımız onaylandığında.
        if (!IsHost && clientId == _netManager.LocalClientId)
        {
            Debug.Log("[Bootstrap] Lobiye bağlanıldı.");
            OnConnected?.Invoke();
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (clientId == _netManager.LocalClientId)
        {
            string reason = _netManager.DisconnectReason;
            if (!string.IsNullOrEmpty(reason))
                Debug.LogWarning($"[Bootstrap] Bağlantı kesildi: {reason}");

            OnDisconnected?.Invoke();
        }
    }
}
