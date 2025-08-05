using System.Collections;
using UnityEngine;

public class AIAttackTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetMarker;
    [SerializeField] private GameObject hammerModel;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float hammerDelay = 0.5f;
    [SerializeField] private float hammerSpeed = 30f;
    [SerializeField] private float attackAngle = 60f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform handTransform;
    [SerializeField] private LayerMask aiLayer;

    private bool isAttacking = false;
    private bool isDead = false;

    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;

    private void Update()
    {
        if (isDead || isAttacking) return;

        if (Time.time - lastAttackTime < attackCooldown) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, aiLayer);
        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < attackAngle / 2f)
            {
                lastAttackTime = Time.time;
                StartAttack(hit.transform.position);
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

        if (isDead) yield break;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
            transform.forward = (targetPos - transform.position).normalized;

        }

        if (targetMarker != null)
        {
            targetMarker.transform.position = targetPos;
            targetMarker.SetActive(true);
        }

        yield return new WaitForSeconds(hammerDelay);
        if (isDead) yield break;

        if (hammerModel != null)
        {
            hammerModel.transform.position = handTransform.position;
            hammerModel.SetActive(true);
        }

        AIPoolManager.Instance.LaunchHammer(handTransform.position, targetPos, hammerSpeed, 1f, this.gameObject);


        yield return null;
        if (!isDead)
        {
            AIFollowPlayer follow = GetComponent<AIFollowPlayer>();
            if (follow != null)
            {
                follow.ResetAttack();
            }
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
    }
}
