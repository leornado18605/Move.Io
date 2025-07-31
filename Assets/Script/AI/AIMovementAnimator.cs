using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AIMovementAnimator : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }

        bool isRunning = agent.velocity.magnitude > 0.1f;
        animator.SetBool("Running", isRunning);
    }
}
