using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAttackTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetMarker;   
    [SerializeField] private GameObject hammerModel;      
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float hammerDelay = 0.5f;
    [SerializeField] private float hammerSpeed = 30f;
    [SerializeField] private LayerMask aiLayer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform handTransform;
    private bool isAttacking = false;

    private void Start()
    {
        targetMarker.GetComponent<MeshRenderer>().enabled = false;
    }

    private IEnumerator HandleAttack(Vector3 targetPos)
    {
        isAttacking = true;
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Attack");

        targetMarker.transform.position = targetPos;
        targetMarker.GetComponent<MeshRenderer>().enabled = true;

        yield return new WaitForSeconds(hammerDelay);
        hammerModel.transform.position = handTransform.position;
        hammerModel.GetComponent<MeshRenderer>().enabled = true;

        AIPoolManager.Instance.LaunchHammer(handTransform.position, targetPos, hammerSpeed, 1f);


        yield return null;
        targetMarker.GetComponent<MeshRenderer>().enabled = false;
        hammerModel.GetComponent<MeshRenderer>().enabled = false;
        isAttacking = false;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void StartAttack(Vector3 aiPosition)
    {
        if(!isAttacking)
        {
            StartCoroutine(HandleAttack(aiPosition));
        }
    }

    public void CancelAttack()
    {
        StopAllCoroutines();
        if (targetMarker != null)
            targetMarker.SetActive(false);
    }

}
