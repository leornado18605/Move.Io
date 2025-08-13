using TMPro;
using UnityEngine;

public class AINameTag : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    private int score;

    public void SetName(string name)
    {
        if (nameText != null)
        {
            nameText.text = name;
            Debug.Log($"Set AINameTag name: {name}");
        }
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
            Debug.Log($"Set AINameTag {nameText.text} score: {score}");
        }

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore(nameText.text, score);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore(nameText.text, score);
        }
    }

    public int GetScore()
    {
        return score;
    }
}