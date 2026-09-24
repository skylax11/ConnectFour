using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardService : APIServices<LeaderboardService>
{
    public IEnumerator GetLeaderboardByScore(Action<ScoreLeaderboardResponse> onComplete, Action<string> onError = null)
    {
        using UnityWebRequest request = UnityWebRequest.Get(_baseUrl + "/api/leaderboard/wins");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        ScoreLeaderboardResponse response = JsonUtility.FromJson<ScoreLeaderboardResponse>(request.downloadHandler.text);
        onComplete?.Invoke(response);
    }

    public IEnumerator GetLeaderboardByTime(Action<TimeLeaderboardResponse> onComplete, Action<string> onError = null)
    {
        using UnityWebRequest request = UnityWebRequest.Get(_baseUrl + "/api/leaderboard/time");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        TimeLeaderboardResponse response = JsonUtility.FromJson<TimeLeaderboardResponse>(request.downloadHandler.text);
        onComplete?.Invoke(response);
    }

    public IEnumerator ReportMatch(int winnerId, float durationMs, Action onComplete = null, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(new MatchResultReport { winnerId = winnerId, durationMs = durationMs });

        using UnityWebRequest request = CreateJsonPost("/api/leaderboard/report", json);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onComplete?.Invoke();
    }
}
