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
        }
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public int GetScore()
    {
        return score;
    }
}