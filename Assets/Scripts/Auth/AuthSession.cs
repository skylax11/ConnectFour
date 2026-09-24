using UnityEngine;

public static class AuthSession
{
    private const string TokenKey = "auth_token";
    private const string UsernameKey = "auth_username";
    private const string UserIdKey = "auth_userid";

    public static string Token { get; private set; }
    public static string Username { get; private set; }
    public static int UserId { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public static void Load()
    {
        Token = PlayerPrefs.GetString(TokenKey, string.Empty);
        Username = PlayerPrefs.GetString(UsernameKey, string.Empty);
        UserId = PlayerPrefs.GetInt(UserIdKey, -1);
    }

    public static void Save(string username, int userId, string token)
    {
        Username = username;
        UserId = userId;
        Token = token;
        PlayerPrefs.SetString(UsernameKey, username);
        PlayerPrefs.SetInt(UserIdKey, userId);
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        Token = string.Empty;
        Username = string.Empty;
        UserId = -1;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.DeleteKey(UserIdKey);
        PlayerPrefs.Save();
    }
}
