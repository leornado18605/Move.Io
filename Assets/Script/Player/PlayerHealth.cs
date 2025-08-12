using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = GameConstants.MAX_HEALTH;

    [Header("References")]
    [SerializeField] private VictoryController victoryController;

    // Cache components
    private Animator cachedAnimator;
    private MoveScript cachedMoveScript;

    // State
    public int currentHealth { get; private set; }
    public bool isDead { get; private set; } = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        cachedAnimator = GetComponent<Animator>();
        cachedMoveScript = GetComponent<MoveScript>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;

        PlayDeathAnimation();
        NotifyGameManager();
    }

    private void PlayDeathAnimation()
    {
        if (cachedAnimator != null)
            cachedAnimator.SetBool(GameConstants.ANIM_DEATH, true);
    }

    private void NotifyGameManager()
    {
        if (GameManager.instance != null)
            GameManager.instance.PlayerDied();
    }
}