using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AuthService : APIServices<AuthService>
{
    [SerializeField]
    private AuthUI _authUI;
    public void asd()
    {

    }
    public IEnumerator LoginRequest(string username, string password)
    {
        string json = JsonUtility.ToJson(new AuthRequest { username = username, password = password });

        using UnityWebRequest req = CreateJsonPost("/api/auth/login", json);
        yield return req.SendWebRequest();

        _authUI.SetInteractable(true);

        if (req.result != UnityWebRequest.Result.Success)
        {
            _authUI.SetStatus(_authUI.ExtractError(req));
            yield break;
        }

        LoginResponse response = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);

        if (response == null || string.IsNullOrEmpty(response.token))
        {
            _authUI.SetStatus("Sunucudan token al�namad�.");
            yield break;
        }

        AuthSession.Save(username, response.userId, response.token);

        SceneManager.LoadScene(_authUI.MainMenuScene);
    }

    public IEnumerator RegisterRequest(string username, string password)
    {
        string json = JsonUtility.ToJson(new AuthRequest { username = username, password = password });

        using UnityWebRequest req = CreateJsonPost("/api/auth/register", json);
        yield return req.SendWebRequest();

        _authUI.SetInteractable(true);

        if (req.result != UnityWebRequest.Result.Success)
        {
            _authUI.SetStatus(_authUI.ExtractError(req));
            yield break;
        }

        _authUI.SetStatus("Kay�t ba�ar�l�, �imdi giri� yapabilirsin.");
    }
}
