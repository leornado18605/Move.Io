using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AIFollowPlayer : MonoBehaviour
{
    private enum AIState
    {
        Idle,
        Move,
        Attack,
        Dead
    }

    private NavMeshAgent agent;

    [Header("Settings")]
    public float stoppingDistance = 2f;
    public float attackRange = 2.5f;
    public float attackAngle = 60f;

    private AIState currentState = AIState.Idle;

    private Transform currentTarget;
    private List<Transform> potentialTargets = new();
    private float targetSwitchTimer = 0f;
    private float targetSwitchCooldown = 3f;

    [SerializeField] Animator animator;

    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private Transform hammerSpawnPoint;
    private int killCount = 0;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = 1.5f;
        agent.acceleration = 4f;
        agent.angularSpeed = 120f;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;

        animator.applyRootMotion = false;
        animator.speed = 1f;
        SetState(AIState.Idle);

    }

    void Update()
    {
        if (currentState == AIState.Dead) return;

        if (currentTarget != null)
        {
            AIFollowPlayer ai = currentTarget.GetComponent<AIFollowPlayer>();
            if (ai != null && ai.currentState == AIState.Dead)
            {
                currentTarget = null;
            }
        }

        targetSwitchTimer += Time.deltaTime;
        if (currentTarget == null || targetSwitchTimer >= targetSwitchCooldown)
        {
            bool shouldSwitch = false;

            if (currentTarget == null)
            {
                shouldSwitch = true;
            }
            else
            {
                var ai = currentTarget.GetComponent<AIFollowPlayer>();
                if (ai == null || ai.currentState == AIState.Dead)
                    shouldSwitch = true;
                else if (Vector3.Distance(transform.position, currentTarget.position) > 200f)
                    shouldSwitch = true;
            }

            if (shouldSwitch)
            {
                currentTarget = FindTarget();
                targetSwitchTimer = 0f;
            }
        }


        switch (currentState)
        {
            case AIState.Idle:
                if (currentTarget) SetState(AIState.Move);
                break;

            case AIState.Move:
                MoveToTarget();
                animator.SetBool("Running", true);
                break;

            case AIState.Attack:
                break;
        }
    }


    private void SetState(AIState newState)
    {
        if (currentState == newState) return;

        // Exit logic
        switch (currentState)
        {
            case AIState.Attack:
                animator.SetBool("Attack", false);
                break;
            case AIState.Move:
                animator.SetBool("Running", false);
                break;
        }

        currentState = newState;

        // Enter logic
        switch (newState)
        {
            case AIState.Idle:
                agent.ResetPath();
                animator.SetBool("Running", false);
                break;

            case AIState.Move:
                if (currentTarget != null)
                {
                    MoveToTarget();

                    animator.SetBool("Running", true);
                }
                break;

            case AIState.Attack:
                agent.ResetPath();
                animator.SetBool("Running", false);
                animator.SetBool("Attack", true);
                Invoke(nameof(ResetAttack), 1f);
                break;

            case AIState.Dead:
                agent.ResetPath();
                agent.isStopped = true;
                animator.SetTrigger("Death");
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

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= attackRange)
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

        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > attackRange)
        {
            SetState(AIState.Move);
        }
        else
        {
            SetState(AIState.Idle);
        }
    }

    private Transform FindTarget()
    {
        potentialTargets.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, 50f);

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            if (hit.CompareTag("Player") || hit.CompareTag("AI"))
            {
                var ai = hit.GetComponent<AIFollowPlayer>();
                if (ai != null && ai.currentState == AIState.Dead) continue;
                potentialTargets.Add(hit.transform);
            }
        }

        if (potentialTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, potentialTargets.Count);
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

        var attackTrigger = GetComponent<AIAttackTrigger>();
        if (attackTrigger != null)
            attackTrigger.SetDead();

        currentTarget = null;
        SetState(AIState.Dead);

        if (killer != null)
        {
            if (killer.CompareTag("Player"))
            {
                ScoreManager.Instance?.AddScore(1);
            }
            else if (killer.CompareTag("AI"))
            {
                var killerAI = killer.GetComponent<AIFollowPlayer>();
                if (killerAI != null)
                {
                    killerAI.IncreaseKill();
                }
            }
        }

        GameManager.instance?.AIDied();

        StartCoroutine(WaitAndReturnToPool(2f));
    }

    public void IncreaseKill()
    {
        killCount++;
        Debug.Log($"{gameObject.name} đã giết {killCount} AI");
    }
    private IEnumerator WaitAndReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset state before pooling
        currentTarget = null;
        currentState = AIState.Idle;

        animator.SetBool("Running", false);
        animator.SetBool("Attack", false);

        agent.enabled = true;
        agent.isStopped = false;

        AIPoolManager.Instance.ReturnAI(gameObject);
    }
  

}
