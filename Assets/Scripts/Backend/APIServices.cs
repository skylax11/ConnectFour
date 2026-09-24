using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIServices<T> : MonoBehaviour where T : APIServices<T>
{
    [Header("Backend")]
    [SerializeField] protected string _baseUrl = "http://localhost:5000";

    public static T Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected UnityWebRequest CreateJsonPost(string path, string json)
    {
        UnityWebRequest req = new UnityWebRequest(_baseUrl + path, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }
}
