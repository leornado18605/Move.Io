using UnityEngine;

public class HammerSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 20f;

    private Rigidbody rb;
    private GameObject owner;

    // Cache components
    private HammerOwner cachedHammerOwner;

    public void SetOwner(GameObject ownerObj) => owner = ownerObj;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Start()
    {
        cachedHammerOwner = GetComponent<HammerOwner>();
    }

    private void OnEnable() => rb.angularVelocity = transform.forward * spinSpeed;

    private void OnTriggerEnter(Collider other)
    {
        GameObject effectiveOwner = GetEffectiveOwner();

        if (ShouldIgnoreCollision(other.gameObject, effectiveOwner))
            return;

        bool shouldDisable = ProcessCollision(other, effectiveOwner);

        if (shouldDisable)
            gameObject.SetActive(false);
    }

    private GameObject GetEffectiveOwner()
    {
        return cachedHammerOwner != null ? cachedHammerOwner.owner : owner;
    }

    private bool ShouldIgnoreCollision(GameObject target, GameObject effectiveOwner)
    {
        return target == effectiveOwner;
    }

    private bool ProcessCollision(Collider other, GameObject effectiveOwner)
    {
        if (other.CompareTag("AI"))
        {
            return ProcessAICollision(other, effectiveOwner);
        }
        else if (other.CompareTag("Player"))
        {
            return ProcessPlayerCollision(other, effectiveOwner);
        }

        return false;
    }

    private bool ProcessAICollision(Collider other, GameObject effectiveOwner)
    {
        AIFollowPlayer ai = other.GetComponent<AIFollowPlayer>();
        if (ai != null && !ai.IsDead())
        {
            ai.Die(effectiveOwner);
            return true;
        }

        return true; // Still disable hammer even if AI is dead
    }

    private bool ProcessPlayerCollision(Collider other, GameObject effectiveOwner)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.isDead)
        {
            playerHealth.TakeDamage(1);
        }

        return true;
    }

    private void OnDisable() => owner = null;


}