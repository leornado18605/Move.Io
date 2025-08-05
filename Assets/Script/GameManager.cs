using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Animator animator;

    [SerializeField] private TMP_Text aliveText;
    private int totalAlive = 0;
    private int aiDeadCount = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void RegisterAI()
    {
        totalAlive++;
        UpdateUI();
    }

    public void AIDied()
    {
        aiDeadCount++;
        totalAlive = Mathf.Max(0, totalAlive - 1);
        Debug.Log("AI chết: " + aiDeadCount);
        UpdateUI();
        ScoreManager.Instance?.AddScore(1);
    }

    private void UpdateUI()
    {
        if (aliveText != null)
        {
            aliveText.text = "Alive: " + totalAlive;
        }
    }
}
