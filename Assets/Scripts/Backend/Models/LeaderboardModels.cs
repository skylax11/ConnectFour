using System;

[Serializable]
public class TimeEntry
{
    public int winnerId;
    public float durationMs;
}

[Serializable]
public class TimeLeaderboardResponse
{
    public TimeEntry[] items;
}

[Serializable]
public class ScoreEntry
{
    public int winnerId;
    public int winCount;
}

[Serializable]
public class ScoreLeaderboardResponse
{
    public ScoreEntry[] items;
}

[Serializable]
public class MatchResultReport
{
    public int winnerId;
    public float durationMs;
}
