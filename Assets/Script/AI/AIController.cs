using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AIFollowPlayer : MonoBehaviour
{
    public event Action<int> OnKillCountChanged;

    private enum AIState
    {
        Idle,
        Move,
        Attack,
        Dead
    }

    private NavMeshAgent agent;

    [Header("Settings")]
    public float stoppingDistance = GameConstants.STOPPING_DISTANCE;
    public float attackRange = GameConstants.ATTACK_RANGE;
    public float attackAngle = GameConstants.ATTACK_ANGLE;

    private AIState currentState = AIState.Idle;

    private Transform currentTarget;
    private List<Transform> potentialTargets = new();
    private float targetSwitchTimer = 0f;
    private float targetSwitchCooldown = GameConstants.AI_TARGET_SWITCH_COOLDOWN;

    [SerializeField] Animator animator;

    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private Transform hammerSpawnPoint;
    public int killCount = 0;

    [SerializeField] private ScoreManager scoreTracker;

    private AIAttackTrigger attackTrigger;

    [Header("Indicator Settings")]
    public Color indicatorColor = Color.red;
    private bool isRegisteredWithIndicator = false;

    public GameObject nameTag;

    // Cache components
    private PlayerController cachedPlayerHealth;
    private AIFollowPlayer cachedAI;

    [SerializeField] private string aiName = "AI";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        attackTrigger = GetComponent<AIAttackTrigger>();

        agent.speed = GameConstants.AI_SPEED;
        agent.acceleration = GameConstants.AI_ACCELERATION;
        agent.angularSpeed = GameConstants.AI_ANGULAR_SPEED;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;

        animator.applyRootMotion = false;
        animator.speed = 1f;
        SetState(AIState.Idle);
        if (string.IsNullOrEmpty(aiName))
            aiName = gameObject.name;
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore(aiName, 0);
        }
    }

    void Update()
    {
        if (currentState == AIState.Dead) return;

        CheckCurrentTarget();
        UpdateTargetIfNeeded();
        ProcessCurrentState();
    }

    private void CheckCurrentTarget()
    {
        if (currentTarget != null)
        {
            if (cachedAI == null)
                cachedAI = currentTarget.GetComponent<AIFollowPlayer>();

            if (cachedAI != null && cachedAI.currentState == AIState.Dead)
            {
                currentTarget = null;
                cachedAI = null;
            }
        }
    }

    private void UpdateTargetIfNeeded()
    {
        targetSwitchTimer += Time.deltaTime;

        if (currentTarget == null || targetSwitchTimer >= targetSwitchCooldown)
        {
            currentTarget = FindTarget();
            targetSwitchTimer = 0f;
            cachedAI = null;
            cachedPlayerHealth = null;
        }

        if (currentTarget != null && currentTarget.CompareTag("Player"))
        {
            if (cachedPlayerHealth == null)
                cachedPlayerHealth = currentTarget.GetComponent<PlayerController>();

            if (cachedPlayerHealth != null && cachedPlayerHealth.isDead)
            {
                currentTarget = null;
                cachedPlayerHealth = null;
                SetState(AIState.Idle);
            }
        }
    }

    
    private void ProcessCurrentState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                if (currentTarget) SetState(AIState.Move);
                break;

            case AIState.Move:
                MoveToTarget();
                break;

            case AIState.Attack:
                break;
        }
    }

    private void SetState(AIState newState)
    {
        if (currentState == newState) return;

        ExitCurrentState();
        currentState = newState;
        EnterNewState(newState);
    }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case AIState.Attack:
                animator.SetBool(GameConstants.ANIM_ATTACK, false);
                break;
            case AIState.Move:
                animator.SetBool(GameConstants.ANIM_RUNNING, false);
                break;
        }
    }

    private void EnterNewState(AIState newState)
    {
        switch (newState)
        {
            case AIState.Idle:
                agent.ResetPath();
                animator.SetBool(GameConstants.ANIM_RUNNING, false);
                break;

            case AIState.Move:
                if (currentTarget != null)
                {
                    MoveToTarget();
                    animator.SetBool(GameConstants.ANIM_RUNNING, true);
                }
                break;

            case AIState.Attack:
                agent.ResetPath();
                animator.SetBool(GameConstants.ANIM_RUNNING, false);
                animator.SetBool(GameConstants.ANIM_ATTACK, true);

                if (currentTarget != null && attackTrigger != null)
                {
                    attackTrigger.StartAttack(currentTarget.position);
                }

                Invoke(nameof(ResetAttack), GameConstants.ATTACK_RESET_DELAY);
                break;

            case AIState.Dead:
                agent.ResetPath();
                agent.isStopped = true;
                animator.SetBool(GameConstants.ANIM_RUNNING, false);
                animator.SetBool(GameConstants.ANIM_ATTACK, false);
                animator.SetBool(GameConstants.ANIM_DEATH, true);
                break;
        }
    }

    private void MoveToTarget()
    {
        if (!currentTarget)
        {
            SetState(AIState.Idle);
            return;
        }

        float sqrDist = (transform.position - currentTarget.position).sqrMagnitude;
        if (sqrDist <= attackRange * attackRange)
        {
            SetState(AIState.Attack);
            return;
        }

        agent.SetDestination(currentTarget.position);

        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, agent.angularSpeed * Time.deltaTime);
        }
    }

    public void ResetAttack()
    {
        if (currentTarget != null)
        {
            float sqrDist = (transform.position - currentTarget.position).sqrMagnitude;
            if (sqrDist > attackRange * attackRange)
            {
                SetState(AIState.Move);
            }
            else
            {
                SetState(AIState.Idle);
            }
        }
        else
        {
            SetState(AIState.Idle);
        }

        animator.SetBool(GameConstants.ANIM_ATTACK, false);
        animator.SetBool(GameConstants.ANIM_RUNNING, currentState == AIState.Move);
    }

    private Transform FindTarget()
    {
        potentialTargets.Clear();
        Transform playerTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, GameConstants.AI_DETECTION_RANGE);

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            if (hit.CompareTag("Player"))
            {
                PlayerController ph = hit.GetComponent<PlayerController>();
                if (ph != null && !ph.isDead)
                {
                    float distToPlayer = Vector3.Distance(transform.position, hit.transform.position);
                    if (distToPlayer <= attackRange)
                    {
                        playerTarget = hit.transform;
                    }
                }
                continue;
            }

            if (hit.CompareTag("AI"))
            {
                AIFollowPlayer ai = hit.GetComponent<AIFollowPlayer>();
                if (ai != null && ai.currentState != AIState.Dead)
                {
                    potentialTargets.Add(hit.transform);
                }
            }
        }

        if (playerTarget != null) return playerTarget;

        if (potentialTargets.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, potentialTargets.Count);
            return potentialTargets[randomIndex];
        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hammer"))
        {
            GameObject killer = null;

            HammerOwner hammerOwner = other.GetComponent<HammerOwner>();
            if (hammerOwner != null)
            {
                killer = hammerOwner.owner;
            }

            Die(killer);
        }
    }

    public void Die(GameObject killer)
    {
        if (currentState == AIState.Dead) return;

        CancelInvoke();

        if (attackTrigger != null)
            attackTrigger.SetDead();

        currentTarget = null;
        SetState(AIState.Dead);

        gameObject.tag = "Untagged";

        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        GameManager.instance?.AIDied(killer);

        StartCoroutine(WaitAndReturnToPool()); // 2f
    }

    public void IncreaseKill()
    {
        killCount++;
        Debug.Log($"AI {gameObject.name} kill count updated to: {killCount}");
        OnKillCountChanged?.Invoke(killCount);
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateScore(aiName, killCount);
        }
        transform.localScale += Vector3.one * GameConstants.KILL_SCALE_AMOUNT;

    }

    private IEnumerator WaitAndReturnToPool()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float deathAnimationLength = 0f;
        if (stateInfo.IsName(GameConstants.ANIM_DEATH.ToString()))
        {
            deathAnimationLength = stateInfo.length / animator.speed;
        }

        // Wait for the death animation to complete plus 1 second
        yield return new WaitForSeconds(deathAnimationLength + 1f);
        Debug.Log($"Reset AI: killCount={killCount}, scale={transform.localScale}, tag={gameObject.tag}, collider enabled={GetComponent<Collider>().enabled}");
        // Deactivate the GameObject
        gameObject.SetActive(false);

        // Reset state before pooling
        currentTarget = null;
        currentState = AIState.Idle;
        cachedAI = null;
        cachedPlayerHealth = null;
        cachedPlayerHealth = null;
        killCount = 0;
        transform.localScale = Vector3.one;
        gameObject.tag = "AI";
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        animator.SetBool(GameConstants.ANIM_RUNNING, false);
        animator.SetBool(GameConstants.ANIM_ATTACK, false);
        animator.SetBool(GameConstants.ANIM_DEATH, false);

        agent.enabled = true;
        agent.isStopped = false;
        gameObject.tag = "AI";
        GetComponent<Collider>().enabled = true;

        AIPoolManager.Instance.ReturnAI(gameObject);
    }

    public Color GetColor()
    {
        return indicatorColor;
    }

    public int GetScore()
    {
        return killCount;
    }

    public bool IsDead()
    {
        return currentState == AIState.Dead;
    }
}