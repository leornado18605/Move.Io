using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Animator animator;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private int aiDeadCount = 0;

    public void AIDied()
    {
        aiDeadCount++;
        Debug.Log("AI chết: " + aiDeadCount);
    }
}
