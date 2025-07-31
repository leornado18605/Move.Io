using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AIFollowPlayer : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    [Header("Settings")]
    public float stoppingDistance = 2f;
    public float attackRange = 2.5f;
    public float updateRate = 0.2f;
    public float attackAngle = 60f;

    private float timer;

    private bool isDead = false;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            agent.SetDestination(player.position);
            timer = 0;
        }
         
        if (agent.velocity.magnitude > 0.1f)
        {
            Vector3 lookDir = agent.steeringTarget - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            }
        }

        animator.SetBool("Running", agent.velocity.magnitude > 0.1f);

        TryAttackPlayer();
    }

    void TryAttackPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            Vector3 toPlayer = (player.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toPlayer);

            if (dot > Mathf.Cos(attackAngle * Mathf.Deg2Rad))
            {
                agent.ResetPath();
                animator.SetTrigger("Attack");
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hammer"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        animator.SetBool("Death", true);

        agent.enabled = false;
        this.enabled = false;

        GameManager.instance?.AIDied();

        StartCoroutine(WaitAndReturnToPool(1f));
    }

    private IEnumerator WaitAndReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);

        isDead = false;
        agent.enabled = true;
        this.enabled = true;

        AIPoolManager.Instance.ReturnAI(gameObject);
    }


}
