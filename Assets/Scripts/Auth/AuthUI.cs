using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    [Header("Scene")]
    public string MainMenuScene = "MainMenu";

    [Header("UI")]
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _registerButton;
    [SerializeField] private TextMeshProUGUI _statusText;

    private void Awake()
    {
        _loginButton.onClick.AddListener(OnLoginClicked);
        _registerButton.onClick.AddListener(OnRegisterClicked);

        SetStatus(string.Empty);
    }

    private void OnLoginClicked()
    {
        if (!TryReadInputs(out string username, out string password))
        {
            return;
        }

        SetInteractable(false);
        SetStatus("Giriş yapılıyor...");


        StartCoroutine(AuthService.Instance.LoginRequest(username, password));
    }

    private void OnRegisterClicked()
    {
        if (!TryReadInputs(out string username, out string password))
        {
            return;
        }

        SetInteractable(false);
        SetStatus("Kayıt olunuyor...");

        StartCoroutine(AuthService.Instance.RegisterRequest(username, password));
    }

    private bool TryReadInputs(out string username, out string password)
    {
        username = _username.text.Trim();
        password = _password.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Kullanıcı adı ve şifre gerekli.");
            return false;
        }

        return true;
    }

    
    public string ExtractError(UnityWebRequest req)
    {
        string body = req.downloadHandler != null ? req.downloadHandler.text : string.Empty;

        if (string.IsNullOrEmpty(body))
        {
            return "Bağlantı hatası: " + req.error;
        }

        return body.Trim('"');
    }

    public void SetInteractable(bool value)
    {
        _loginButton.interactable = value;
        _registerButton.interactable = value;
    }

    public void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }
}

[Serializable]
public class AuthRequest
{
    public string username;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string token;
    public int userId;
}
