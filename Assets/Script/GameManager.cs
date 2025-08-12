using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Animator animator;
    [SerializeField] private TMP_Text aliveText;
    [SerializeField] private PlayerHealth playerHealth;

    private int totalAlive = 1;
    private int aiDeadCount = 0;

    // Cache components
    private ScoreManager cachedScoreManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Cache components
        cachedScoreManager = ScoreManager.Instance;
    }

    public void RegisterAI()
    {
        totalAlive++;
        UpdateUI();
    }

    public void AIDied(GameObject killer)
    {
        aiDeadCount++;
        totalAlive = Mathf.Max(0, totalAlive - 1);

        UpdateUI();
        ProcessKiller(killer);
        CheckWinCondition();
    }

    private void ProcessKiller(GameObject killer)
    {
        if (killer == null) return;

        if (killer.CompareTag("Player"))
        {
            if (cachedScoreManager != null)
            {
                cachedScoreManager.AddScore(1);
            }
        }
        else if (killer.CompareTag("AI"))
        {
            ProcessAIKiller(killer);
        }

    }

    private void ProcessAIKiller(GameObject killer)
    {
        AIFollowPlayer aiKiller = killer.GetComponent<AIFollowPlayer>();
        if (aiKiller != null && !aiKiller.IsDead())
        {
            //aiKiller.AddScore(1);
            aiKiller.IncreaseKill();

            UpdateAINameTag(aiKiller);
        }
    }

    private void UpdateAINameTag(AIFollowPlayer aiKiller)
    {
        if (aiKiller.nameTag != null)
        {
            AINameTag nameTag = aiKiller.nameTag.GetComponent<AINameTag>();
            if (nameTag != null)
            {
                nameTag.SetScore(aiKiller.GetScore());
            }
        }
    }

    private void CheckWinCondition()
    {
        if (totalAlive == 1)
        {
            if (playerHealth != null && !playerHealth.isDead)
            {
                animator.SetBool(GameConstants.ANIM_VICTORY, true);
            }
            else
            {
                SceneManager.LoadScene(GameConstants.SCENE_LOSE);
            }
        }
    }

    private void UpdateUI()
    {
        if (aliveText != null)
        {
            aliveText.text = string.Format(GameConstants.ALIVE_TEXT_FORMAT, totalAlive);
        }
    }

    public void PlayerDied()
    {
        SceneManager.LoadScene(GameConstants.SCENE_LOSE);
    }
}