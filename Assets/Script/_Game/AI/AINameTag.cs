using TMPro;
using UnityEngine;

public class AINameTag : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Settings")]
    [SerializeField] private string defaultNamePrefix = "AI_";

    private string aiName;
    private int score;

    public void Initialize(string name = null, int initialScore = 0)
    {
        SetName(string.IsNullOrEmpty(name) ? defaultNamePrefix + Random.Range(100, 1000) : name);
        SetScore(initialScore);
    }

    public void SetName(string name)
    {
        aiName = name;
        if (nameText != null)
        {
            nameText.text = aiName;
        }
        UpdateLeaderboard();
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        UpdateScoreDisplay();
        UpdateLeaderboard();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
        UpdateLeaderboard();
    }

    public string GetName() => aiName;
    public int GetScore() => score;

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateLeaderboard()
    {
        if (LeaderboardManager.Instance != null && !string.IsNullOrEmpty(aiName))
        {
            LeaderboardManager.Instance.UpdateScore(aiName, score);
        }
    }

    private void OnDestroy()
    {
        if (LeaderboardManager.Instance != null && !string.IsNullOrEmpty(aiName))
        {
            LeaderboardManager.Instance.UpdateScore(aiName, 0); 
        }
    }
}