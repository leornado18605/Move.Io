using UnityEngine;
using DG.Tweening; // Thêm DOTween

public class HammerSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 360f;
    private GameObject owner;
    private HammerOwner cachedHammerOwner;
    private Tween spinTween;

    public void SetOwner(GameObject ownerObj) => owner = ownerObj;

    private void Start()
    {
        cachedHammerOwner = GetComponent<HammerOwner>();
    }

    private void OnEnable()
    {
        spinTween = transform
            .DORotate(new Vector3(0, 360, 0), 1f / (spinSpeed / 360f), RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDisable()
    {
        owner = null;
        spinTween?.Kill(); 
    }

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

        return true;
    }

    private bool ProcessPlayerCollision(Collider other, GameObject effectiveOwner)
    {
        PlayerController playerHealth = other.GetComponent<PlayerController>();
        if (playerHealth != null && !playerHealth.isDead)
        {
            playerHealth.TakeDamage(1);
        }

        return true;
    }
}
