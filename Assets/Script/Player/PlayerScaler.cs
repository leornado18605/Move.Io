using UnityEngine;

public class PlayerScaler : MonoBehaviour
{
    private int lastScaleScore = 0;
    public int scaleStep = 5;
    public float scaleAmount = 0.1f;
    public Transform scaleTarget;

    private void Start()
    {
        if (scaleTarget == null)
            scaleTarget = transform;

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
        ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }
}
