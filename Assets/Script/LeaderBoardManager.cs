using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    //private void Start()
    //{
    //    var players = new List<PlayerData>
    //{
    //    new PlayerData("AI-1", 01),
    //    new PlayerData("AI-2", 00),
    //    new PlayerData("AI-3", 00),
    //    new PlayerData("AI-4", 00),
    //    new PlayerData("AI-5", 00),
    //    new PlayerData("AI-6", 00),
    //};
    //    UpdateLeaderboard(players);
    //}
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

    public void UpdateLeaderboard(List<PlayerData> players)
    {
        leaderboard.Clear();
        foreach (var p in players)
        {
            leaderboard.Add(new LeaderboardEntry { name = p.name, score = p.score });
        }
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        var ui = FindObjectOfType<LeaderboardUI>();
        if (ui != null) ui.RefreshUI();
    }

    public void UpdateScore(string playerName, int addedScore)
    {
        var entry = leaderboard.Find(e => e.name == playerName);
        if (entry != null)
        {
            entry.score += addedScore;
        }
        else
        {
            leaderboard.Add(new LeaderboardEntry { name = playerName, score = addedScore });
        }
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        var ui = FindObjectOfType<LeaderboardUI>();
        if (ui != null) ui.RefreshUI();
    }

    public List<LeaderboardEntry> GetLeaderboard()
    {
        return leaderboard;
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int score;
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


