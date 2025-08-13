using System.Collections;
using UnityEngine;

public class AIAttackTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetMarker;
    [SerializeField] private GameObject hammerModel;
    [SerializeField] private float attackRange = GameConstants.ATTACK_RANGE * 2; // 5f
    [SerializeField] private float hammerDelay = GameConstants.HAMMER_DELAY;
    [SerializeField] private float hammerSpeed = GameConstants.HAMMER_SPEED;
    [SerializeField] private float attackAngle = GameConstants.ATTACK_ANGLE;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform handTransform;
    [SerializeField] private LayerMask aiLayer;

    private bool isAttacking = false;
    private bool isDead = false;
    [SerializeField] private float attackCooldown = GameConstants.ATTACK_COOLDOWN;
    private float lastAttackTime = -Mathf.Infinity;

    // Cache components
    private AIFollowPlayer cachedAIFollow;

    private void Start()
    {
        cachedAIFollow = GetComponent<AIFollowPlayer>();
    }

    private void Update()
    {
        if (isDead || isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        CheckForTargetsInRange();
    }

    private void CheckForTargetsInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, aiLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject == this.gameObject) continue;

            Vector3 dirToTarget = (hits[i].transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < attackAngle / 2f)
            {
                lastAttackTime = Time.time;
                StartAttack(hits[i].transform.position);
                break;
            }
        }
    }

    public void StartAttack(Vector3 targetPos)
    {
        if (!isAttacking && !isDead)
            StartCoroutine(HandleAttack(targetPos));
    }

    private IEnumerator HandleAttack(Vector3 targetPos)
    {
        isAttacking = true;

        if (isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (animator != null)
        {
            animator.SetTrigger(GameConstants.ANIM_ATTACK);
            transform.forward = (targetPos - transform.position).normalized;
        }

        if (targetMarker != null)
        {
            targetMarker.transform.position = targetPos;
            targetMarker.SetActive(true);
        }

        yield return new WaitForSeconds(hammerDelay);

        if (isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (hammerModel != null)
        {
            hammerModel.transform.position = handTransform.position;
            hammerModel.SetActive(true);
        }

        AIPoolManager.Instance.LaunchHammer(handTransform, targetPos, hammerSpeed, GameConstants.ATTACK_RESET_DELAY, gameObject);

        yield return null;

        if (!isDead && cachedAIFollow != null)
        {
            cachedAIFollow.ResetAttack();
        }

        if (targetMarker != null)
            targetMarker.SetActive(false);
        if (hammerModel != null)
            hammerModel.SetActive(false);

        isAttacking = false;
    }

    public void SetDead()
    {
        isDead = true;
        StopAllCoroutines();
        isAttacking = false;
    }
}