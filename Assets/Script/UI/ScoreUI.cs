using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;

    // Cache
    private ScoreManager cachedScoreManager;

    private void Start()
    {
        InitializeScoreManager();
        SubscribeToScoreEvents();
        UpdateInitialScore();
    }

    private void InitializeScoreManager()
    {
        cachedScoreManager = ScoreManager.Instance;
    }

    private void SubscribeToScoreEvents()
    {
        if (cachedScoreManager != null)
        {
            cachedScoreManager.OnScoreChanged += UpdateScoreText;
        }
    }

    private void UpdateInitialScore()
    {
        if (cachedScoreManager != null)
        {
            UpdateScoreText(cachedScoreManager.Score);
        }
    }

    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromScoreEvents();
    }

    private void UnsubscribeFromScoreEvents()
    {
        if (cachedScoreManager != null)
        {
            cachedScoreManager.OnScoreChanged -= UpdateScoreText;
        }
    }
}