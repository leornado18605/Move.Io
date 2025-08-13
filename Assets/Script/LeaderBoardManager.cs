using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [SerializeField] private int maxDisplayedEntries = 5;
    private List<LeaderboardData> leaderboard = new List<LeaderboardData>();

    public event Action<List<LeaderboardData>> OnLeaderboardUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateScore(string playerName, int addedScore)
    {
        var entry = leaderboard.Find(e => e.playerName == playerName);
        if (entry != null)
        {
            entry.playerScore += addedScore;
        }
        else
        {
            leaderboard.Add(new LeaderboardData { playerName = playerName, playerScore = addedScore });
        }

        leaderboard.Sort((a, b) => b.playerScore.CompareTo(a.playerScore));
        if (leaderboard.Count > maxDisplayedEntries)
        {
            leaderboard = leaderboard.GetRange(0, maxDisplayedEntries);
        }

        NotifyUpdated();
    }

    public List<LeaderboardData> GetTopEntries()
    {
        return leaderboard.Count > maxDisplayedEntries ?
               leaderboard.GetRange(0, maxDisplayedEntries) :
               new List<LeaderboardData>(leaderboard);
    }

    private void NotifyUpdated()
    {
        var snapshot = GetTopEntries();
        OnLeaderboardUpdated?.Invoke(snapshot);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshLeaderboardUI(snapshot);
        }
    }

    public void UpdateLeaderboard(List<PlayerData> players)
    {
        leaderboard.Clear();
        foreach (var p in players)
        {
            leaderboard.Add(new LeaderboardData
            {
                playerName = p.name,
                playerScore = p.score
            });
        }
        leaderboard.Sort((a, b) => b.playerScore.CompareTo(a.playerScore));

        NotifyUpdated();
    }
}

[System.Serializable]
public class LeaderboardData
{
    public string playerName;
    public int playerScore;
}

[System.Serializable]
public class PlayerData
{
    public string name;
    public int score;

    public PlayerData(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}