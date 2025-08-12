using System.Collections;
using UnityEngine;

public class PlayerAttackTrigger : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject targetMarker;
    [SerializeField] private GameObject hammerModel;
    [SerializeField] private float attackRange = GameConstants.ATTACK_RANGE;
    [SerializeField] private float hammerDelay = GameConstants.HAMMER_DELAY;
    [SerializeField] private float hammerSpeed = GameConstants.HAMMER_SPEED;
    [SerializeField] private LayerMask aiLayer;

    [Header("References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform handTransform;

    // Cache components
    private MeshRenderer cachedTargetMarker;
    private MeshRenderer cachedHammerModel;
    private Transform cachedTransform;

    // State
    private bool isAttacking = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        cachedTargetMarker = targetMarker.GetComponent<MeshRenderer>();
        cachedHammerModel = hammerModel.GetComponent<MeshRenderer>();
        cachedTransform = transform;

        cachedTargetMarker.enabled = false;
    }

    public void StartAttack(Vector3 aiPosition)
    {
        if (!isAttacking)
        {
            StartCoroutine(HandleAttack(aiPosition));
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
    }

    private void TriggerPlayerAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(GameConstants.ANIM_ATTACK);
        }
    }

    private void ShowTargetMarker(Vector3 targetPos)
    {
        targetMarker.transform.position = targetPos;
        cachedTargetMarker.enabled = true;
    }

    private void LaunchHammer(Vector3 targetPos)
    {
        hammerModel.transform.position = handTransform.position;
        cachedHammerModel.enabled = true;

        if (AIPoolManager.Instance != null)
        {
            AIPoolManager.Instance.LaunchHammer(
                handTransform.position,
                targetPos,
                hammerSpeed,
                1f,
                gameObject
            );
        }
    }

    private void HideAttackVisuals()
    {
        cachedTargetMarker.enabled = false;
        cachedHammerModel.enabled = false;
    }

    public void CancelAttack()
    {
        StopAllCoroutines();
        if (targetMarker != null)
            targetMarker.SetActive(false);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}