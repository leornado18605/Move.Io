using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HammerSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 20f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        rb.angularVelocity = transform.forward * spinSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("AI"))
        {
            AIFollowPlayer ai = other.GetComponent<AIFollowPlayer>();
            if (ai != null)
            {
                ai.Die();
            }
            gameObject.SetActive(false);
        }
    }
}
