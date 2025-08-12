using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MoveScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = GameConstants.PLAYER_MOVE_SPEED;
    [SerializeField] private float gravity = GameConstants.GRAVITY;

    [Header("Detection Settings")]
    [SerializeField] private float detectRange = GameConstants.DETECT_RANGE;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerAttackTrigger playerAttack;

    // Cache components
    private PlayerHealth cachedPlayerHealth;
    private Transform cachedTransform;

    // State variables
    private Vector3 velocity;
    private bool wasMoving = false;
    private bool isAttacking = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        controller.stepOffset = GameConstants.STEP_OFFSET;
        cachedPlayerHealth = GetComponent<PlayerHealth>();
        cachedTransform = transform;
    }

    private void Update()
    {
        HandleMovement();
        CheckMovementState();
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
        Collider[] hitAI = Physics.OverlapSphere(cachedTransform.position, detectRange);
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

        TriggerAttack();
        playerAttack.StartAttack(aiTransform.position);
    }

    private void RotateToTarget(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        cachedTransform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }

    private void TriggerAttack()
    {
        if (animator != null)
            animator.SetTrigger(GameConstants.ANIM_ATTACK);
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
            animator.Play(GameConstants.ANIM_IDLE);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}