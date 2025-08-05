using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MoveScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Detection Settings")]
    [SerializeField] private float detectRange = 2f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController controller;

    private Vector3 velocity;
    
    private bool wasMoving = false;

    //Attack
    [SerializeField] private PlayerAttackTrigger playerAttack;
    private bool isAttacking = false;

    private void Awake()
    {
        controller.stepOffset = 0.4f;
    }

    private void Update()
    {
        HandleMovement();
        if (JoystickControl.direct.sqrMagnitude == 0 && wasMoving)
        {
            wasMoving = false;
            if (animator) animator.SetBool("Running", false);
            if(!isAttacking)
                HandleDetectionAndAttack();
        }

    }

    void HandleMovement()
    {
        Vector3 input = JoystickControl.direct;
        Vector3 moveDirection = new Vector3(input.x, 0f, input.z).normalized;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            if (animator) animator.SetBool("Running", true);
            wasMoving = true;
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleDetectionAndAttack()
    {
        Collider[] hitAI = Physics.OverlapSphere(transform.position, detectRange);
        Transform nearestAI = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in hitAI)
        {
            if (col.CompareTag("AI"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    nearestAI = col.transform;
                    minDistance = dist;
                }
            }
        }

        if (nearestAI != null)
        {
            Vector3 toAI = (nearestAI.position - transform.position).normalized;
            Vector3 forward = transform.forward;

            float angle = Vector3.Angle(forward, toAI);
            if (angle < 60f)
            {
                if (animator) animator.SetTrigger("Attack");
            }
            else
            {
                float targetAngle = Mathf.Atan2(toAI.x, toAI.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

                if (animator) animator.SetTrigger("Attack");
                playerAttack.StartAttack(nearestAI.position);
            }
        }
        else
        {
            if (animator) animator.Play("Idle");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

}
