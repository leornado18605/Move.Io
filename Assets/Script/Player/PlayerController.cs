using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = GameConstants.PLAYER_MOVE_SPEED;
    [SerializeField] private float gravity = GameConstants.GRAVITY;
    [SerializeField] private float detectRange = GameConstants.DETECT_RANGE;

    [Header("Attack Settings")]
    [SerializeField] private GameObject targetMarker;
    [SerializeField] private GameObject hammerModel;
    [SerializeField] private float attackRange = GameConstants.ATTACK_RANGE;
    [SerializeField] private float hammerDelay = GameConstants.HAMMER_DELAY;
    [SerializeField] private float hammerSpeed = GameConstants.HAMMER_SPEED;
    [SerializeField] private LayerMask aiLayer;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = GameConstants.MAX_HEALTH;
    [SerializeField] private VictoryController victoryController;

    [Header("Scale Settings")]
    [SerializeField] private int scaleStep = GameConstants.SCALE_STEP;
    [SerializeField] private float scaleAmount = GameConstants.SCALE_AMOUNT;
    [SerializeField] private Transform scaleTarget;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform handTransform;

    // Cache components
    private MeshRenderer cachedTargetMarker;
    private MeshRenderer cachedHammerModel;
    private Transform cachedTransform;
    private Vector3 velocity;
    private bool wasMoving = false;
    private bool isAttacking = false;
    private int currentHealth;
    public bool isDead = false;
    private int lastScaleScore = 0;

    private Coroutine attackCoroutine;
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        InitializeScaleTarget();
        SubscribeToScoreEvents();
        StartCoroutine(CheckAttackState());

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore("Player", 0);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromScoreEvents();
    }

    private void InitializeComponents()
    {
        controller.stepOffset = GameConstants.STEP_OFFSET;
        cachedTransform = transform;
        if (targetMarker != null)
            cachedTargetMarker = targetMarker.GetComponent<MeshRenderer>();
        if (hammerModel != null)
            cachedHammerModel = hammerModel.GetComponent<MeshRenderer>();
        if (cachedTargetMarker != null)
            cachedTargetMarker.enabled = false;
    }

    private void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            CheckMovementState();
        }
    }

    private void HandleMovement()
    {
        Vector3 input = JoystickControl.direct;
        Vector3 moveDirection = new Vector3(input.x, 0f, input.z).normalized;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        if (moveDirection.sqrMagnitude >= 0.01f) // Use sqrMagnitude for performance
        {
            RotateToMovementDirection(moveDirection);
            SetRunningAnimation(true);
            wasMoving = true;

            if (isAttacking)
            {
                CancelAttack();
            }
        }
        ApplyGravity();
    }

    private void RotateToMovementDirection(Vector3 moveDirection)
    {
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        cachedTransform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    private void SetRunningAnimation(bool isRunning)
    {
        if (animator != null)
            animator.SetBool(GameConstants.ANIM_RUNNING, isRunning);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = GameConstants.GROUND_CHECK;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckMovementState()
    {
        if (JoystickControl.direct.sqrMagnitude == 0 && wasMoving)
        {
            wasMoving = false;
            SetRunningAnimation(false);
            if (!isAttacking)
                HandleDetectionAndAttack();
        }
    }

    private IEnumerator CheckAttackState()
    {
        while (!isDead)
        {
            if (!isAttacking && JoystickControl.direct.sqrMagnitude == 0)
            {
                HandleDetectionAndAttack();
            }
            yield return null;
        }
    }

    private void HandleDetectionAndAttack()
    {
        Transform nearestAI = FindNearestAI();
        if (nearestAI != null)
        {
            AttackNearestAI(nearestAI);
        }
        else
        {
            PlayIdleAnimation();
        }
    }

    private Transform FindNearestAI()
    {
        Collider[] hitAI = Physics.OverlapSphere(cachedTransform.position, detectRange, aiLayer);
        Transform nearestAI = null;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < hitAI.Length; i++)
        {
            if (hitAI[i].CompareTag(GameConstants.TAG_AI))
            {
                float distance = Vector3.Distance(cachedTransform.position, hitAI[i].transform.position);
                if (distance < minDistance)
                {
                    nearestAI = hitAI[i].transform;
                    minDistance = distance;
                }
            }
        }
        return nearestAI;
    }

    private void AttackNearestAI(Transform aiTransform)
    {
        Vector3 toAI = (aiTransform.position - cachedTransform.position).normalized;
        Vector3 forward = cachedTransform.forward;
        float angle = Vector3.Angle(forward, toAI);
        if (angle >= GameConstants.ATTACK_ANGLE)
        {
            RotateToTarget(toAI);
        }
        StartAttack(aiTransform.position);
    }

    private void RotateToTarget(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        cachedTransform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    private void StartAttack(Vector3 aiPosition)
    {
        if (!isAttacking)
        {
            attackCoroutine = StartCoroutine(HandleAttack(aiPosition));
        }
    }

    private IEnumerator HandleAttack(Vector3 targetPos)
    {
        isAttacking = true;
        TriggerPlayerAnimation();
        ShowTargetMarker(targetPos);
        yield return new WaitForSeconds(hammerDelay);
        LaunchHammer(targetPos);
        HideAttackVisuals();
        isAttacking = false;

        attackCoroutine = null;
    }

    private void TriggerPlayerAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(GameConstants.ANIM_ATTACK);
        }
    }

    private void ShowTargetMarker(Vector3 targetPos)
    {
        if (targetMarker != null)
        {
            targetMarker.transform.position = targetPos;
            cachedTargetMarker.enabled = true;
        }
    }

    private void LaunchHammer(Vector3 targetPos)
    {
        if (hammerModel != null)
        {
            hammerModel.transform.position = handTransform.position;
            cachedHammerModel.enabled = true;
        }
        if (AIPoolManager.Instance != null)
        {
            AIPoolManager.Instance.LaunchHammer(
                handTransform,
                targetPos,
                hammerSpeed,
                1f,
                gameObject
            );
        }
    }

    private void HideAttackVisuals()
    {
        if (cachedTargetMarker != null)
            cachedTargetMarker.enabled = false;
        if (cachedHammerModel != null)
            cachedHammerModel.enabled = false;
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
            animator.Play(GameConstants.ANIM_IDLE);
    }

    public void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        HideAttackVisuals();
        isAttacking = false;
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
        if (animator != null)
            animator.SetBool(GameConstants.ANIM_DEATH, true);
    }

    private void NotifyGameManager()
    {
        if (GameManager.instance != null)
            GameManager.instance.PlayerDied();
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

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore("Player", newScore);
        }
    }

    private void UnsubscribeFromScoreEvents()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}