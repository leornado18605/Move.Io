using UnityEngine;

public class PlayerScaler : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private int scaleStep = GameConstants.SCALE_STEP;
    [SerializeField] private float scaleAmount = GameConstants.SCALE_AMOUNT;
    [SerializeField] private Transform scaleTarget;

    // Cache
    private int lastScaleScore = 0;

    private void Start()
    {
        InitializeScaleTarget();
        SubscribeToScoreEvents();
    }

    private void InitializeScaleTarget()
    {
        if (scaleTarget == null)
            scaleTarget = transform;
    }

    private void SubscribeToScoreEvents()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
    }

    private void HandleScoreChanged(int newScore)
    {
        if (newScore - lastScaleScore >= scaleStep)
        {
            lastScaleScore = newScore;
            scaleTarget.localScale += Vector3.one * scaleAmount;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromScoreEvents();
    }

    private void UnsubscribeFromScoreEvents()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }
}