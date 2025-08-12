using UnityEngine;

public class HammerOwner : MonoBehaviour
{
    public GameObject owner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AI") || other.CompareTag("Player"))
        {
            if (other.gameObject != owner)
            {
                if (other.CompareTag("AI"))
                {
                    AIFollowPlayer ai = other.GetComponent<AIFollowPlayer>();
                    if (ai != null)
                    {
                        ai.Die(owner);
                    }
                }
                else if (other.CompareTag("Player"))
                {
                    PlayerHealth player = other.GetComponent<PlayerHealth>();
                    if (player != null)
                    {
                        player.Die();
                    }
                }
            }
        }
    }
}