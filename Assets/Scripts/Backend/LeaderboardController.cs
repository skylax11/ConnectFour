using System.Text;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private MainMenuUI _mainMenu;

    private void OnEnable()
    {
        _mainMenu.OnWinCountLeaderboardRequested += HandleWinCountRequested;
        _mainMenu.OnTimeLeaderboardRequested += HandleTimeRequested;
    }

    private void OnDisable()
    {
        _mainMenu.OnWinCountLeaderboardRequested -= HandleWinCountRequested;
        _mainMenu.OnTimeLeaderboardRequested -= HandleTimeRequested;
    }

    private void HandleWinCountRequested()
    {
        _mainMenu.SetLeaderboardText("Yükleniyor...");

        StartCoroutine(LeaderboardService.Instance.GetLeaderboardByScore(
            response => _mainMenu.SetLeaderboardText(FormatScore(response)),
            error => _mainMenu.SetLeaderboardText("Hata: " + error)));
    }

    private void HandleTimeRequested()
    {
        _mainMenu.SetLeaderboardText("Yükleniyor...");

        StartCoroutine(LeaderboardService.Instance.GetLeaderboardByTime(
            response => _mainMenu.SetLeaderboardText(FormatTime(response)),
            error => _mainMenu.SetLeaderboardText("Hata: " + error)));
    }

    private string FormatScore(ScoreLeaderboardResponse response)
    {
        if (response == null || response.items == null || response.items.Length == 0)
        {
            return "Henüz kayıt yok.";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("En çok galibiyet");

        for (int i = 0; i < response.items.Length; i++)
        {
            ScoreEntry entry = response.items[i];
            sb.AppendLine($"{i + 1}. Oyuncu {entry.winnerId} - {entry.winCount}");
        }

        return sb.ToString();
    }

    private string FormatTime(TimeLeaderboardResponse response)
    {
        if (response == null || response.items == null || response.items.Length == 0)
        {
            return "Henüz kayıt yok.";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("En hızlı galibiyet");

        for (int i = 0; i < response.items.Length; i++)
        {
            TimeEntry entry = response.items[i];
            sb.AppendLine($"{i + 1}. Oyuncu {entry.winnerId} - {entry.durationMs / 1000f:F2} sn");
        }

        return sb.ToString();
    }
}
