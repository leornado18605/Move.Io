using UnityEngine;

public class HammerSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 20f;

    private Rigidbody rb;
    private GameObject owner;

    public void SetOwner(GameObject ownerObj)
    {
        owner = ownerObj;
    }

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
        if (other.gameObject == owner) return;
        if (other.CompareTag("AI") && other.gameObject != owner)
        {
            AIFollowPlayer ai = other.GetComponent<AIFollowPlayer>();
            if (ai != null)
            {
                ai.Die(owner);
            }

            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        owner = null; // reset
    }
}
