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
    private int killCount = 0;

    [SerializeField] private ScoreManager scoreTracker;

    private AIAttackTrigger attackTrigger;

    //private int score = 0;
    [SerializeField] private ScoreUI scoreUI;

    [Header("Indicator Settings")]
    public Color indicatorColor = Color.red;
    private bool isRegisteredWithIndicator = false;

    public GameObject nameTag;

    // Cache components
    private PlayerHealth cachedPlayerHealth;
    private AIFollowPlayer cachedAI;

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

        if (ShouldSwitchTarget())
        {
            currentTarget = FindTarget();
            targetSwitchTimer = 0f;
            cachedAI = null;
            cachedPlayerHealth = null;
        }

        CheckPlayerTarget();
    }

    private bool ShouldSwitchTarget()
    {
        if (currentTarget == null || targetSwitchTimer >= targetSwitchCooldown)
        {
            if (currentTarget == null) return true;

            if (cachedAI != null && cachedAI.currentState == AIState.Dead) return true;

            float sqrDistance = (transform.position - currentTarget.position).sqrMagnitude;
            if (sqrDistance > GameConstants.AI_MAX_DISTANCE * GameConstants.AI_MAX_DISTANCE) return true;
        }

        return false;
    }

    private void CheckPlayerTarget()
    {
        if (currentTarget != null && currentTarget.CompareTag("Player"))
        {
            if (cachedPlayerHealth == null)
                cachedPlayerHealth = currentTarget.GetComponent<PlayerHealth>();

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
        Collider[] hits = Physics.OverlapSphere(transform.position, GameConstants.AI_DETECTION_RANGE);

        // Prioritize Player
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform) continue;

            if (hits[i].CompareTag("Player"))
            {
                PlayerHealth playerHealth = hits[i].GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.isDead)
                {
                    return hits[i].transform;
                }
            }
        }

        // Find AI targets
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == transform) continue;

            if (hits[i].CompareTag("AI"))
            {
                AIFollowPlayer ai = hits[i].GetComponent<AIFollowPlayer>();
                if (ai != null && ai.currentState != AIState.Dead)
                {
                    potentialTargets.Add(hits[i].transform);
                }
            }
        }

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

        StartCoroutine(WaitAndReturnToPool(GameConstants.HAMMER_RETURN_DELAY * 4)); // 2f
    }

    //public void AddScore(int amount)
    //{
    //    score += amount;

    //    if (nameTag != null)
    //    {
    //        AINameTag tag = nameTag.GetComponent<AINameTag>();
    //        if (tag != null)
    //        {
    //            tag.SetScore(score);
    //        }
    //    }
    //}

    public void IncreaseKill()
    {
        killCount++;
        OnKillCountChanged?.Invoke(killCount);

        transform.localScale += Vector3.one * GameConstants.KILL_SCALE_AMOUNT;

    }

    private IEnumerator WaitAndReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset state before pooling
        currentTarget = null;
        currentState = AIState.Idle;
        cachedAI = null;
        cachedPlayerHealth = null;

        animator.SetBool(GameConstants.ANIM_RUNNING, false);
        animator.SetBool(GameConstants.ANIM_ATTACK, false);

        agent.enabled = true;
        agent.isStopped = false;

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