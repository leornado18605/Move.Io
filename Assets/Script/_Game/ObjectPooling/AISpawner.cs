using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            AIPoolManager.Instance.StartCoroutine("SpawnAIInAreas");
        }
    }

    public void SpawnNow()
    {
        AIPoolManager.Instance.StartCoroutine("SpawnAIInAreas");
    }
}
