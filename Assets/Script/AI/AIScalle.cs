using UnityEngine;

public class AIScaler : MonoBehaviour
{
    private int lastScaleKillCount = 0;
    public int scaleStep = GameConstants.SCALE_STEP;
    public float scaleAmount = GameConstants.SCALE_AMOUNT;
    public Transform scaleTarget;

    private AIFollowPlayer aiFollow;

    private void Start()
    {
        if (scaleTarget == null)
            scaleTarget = transform;

        aiFollow = GetComponent<AIFollowPlayer>();
        if (aiFollow != null)
        {
            aiFollow.OnKillCountChanged += HandleKillCountChanged;
        }
    }

    private void HandleKillCountChanged(int newKillCount)
    {
        if (newKillCount - lastScaleKillCount >= scaleStep)
        {
            lastScaleKillCount = newKillCount;
            scaleTarget.localScale += Vector3.one * scaleAmount;
        }
    }

    private void OnDestroy()
    {
        if (aiFollow != null)
        {
            aiFollow.OnKillCountChanged -= HandleKillCountChanged;
        }
    }
}