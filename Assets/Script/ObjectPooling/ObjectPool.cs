using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPoolManager : MonoBehaviour
{
    public static AIPoolManager Instance;

    [Header("AI Settings")]
    [SerializeField] private List<GameObject> aiPrefabs;
    [SerializeField] private int aiPerType = 10;
    [SerializeField] private List<Transform> spawnAreas;

    [Header("Hammer Settings")]
    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private int hammerPoolSize = 5;


    private readonly Dictionary<string, List<GameObject>> aiPools = new();
    private readonly List<Vector3> usedSpawnPositions = new();
    private readonly List<GameObject> hammerPool = new();

    [Header("NameTag Settings")]
    [SerializeField] private GameObject aiNameTagPrefab;

    private void Awake()
    {
        Instance = this;
        CreateAIPools();
        CreateHammerPool();
    }

    private void Start()
    {
        StartCoroutine(SpawnAIInAreas());
    }

    private void CreateAIPools()
    {
        foreach (GameObject prefab in aiPrefabs)
        {
            List<GameObject> pool = new();
            for (int i = 0; i < aiPerType; i++)
            {
                GameObject ai = Instantiate(prefab, transform);
                ai.SetActive(false);
                pool.Add(ai);
            }
            aiPools[prefab.name] = pool;
        }
    }

    private void CreateHammerPool()
    {
        for (int i = 0; i < hammerPoolSize; i++)
        {
            GameObject hammer = Instantiate(hammerPrefab, transform);
            hammer.SetActive(false);
            hammerPool.Add(hammer);
        }
    }

    private IEnumerator SpawnAIInAreas()
    {
        usedSpawnPositions.Clear();

        foreach (Transform area in spawnAreas)
        {
            for (int i = 0; i < aiPerType; i++)
            {
                GameObject prefab = aiPrefabs[Random.Range(0, aiPrefabs.Count)];
                Vector3 spawnPos = GetValidSpawnPoint(area, 15f);

                GameObject ai = GetAIFromPool(prefab.name);
                if (ai == null) continue;

                ai.transform.position = spawnPos;
                ai.SetActive(true);
                GameObject nameTag = Instantiate(aiNameTagPrefab, Vector3.zero, Quaternion.identity);
                UIFollowHead followHead = nameTag.GetComponent<UIFollowHead>();
                if (followHead != null)
                {
                    Transform aiHead = ai.transform.Find("Head"); 
                    if (aiHead == null) aiHead = ai.transform; 
                    followHead.target = aiHead;
                    followHead.offset = new Vector3(0, 1.5f, 0);  
                }

                AINameTag tagScript = nameTag.GetComponent<AINameTag>();
                if (tagScript != null)
                {
                    string aiName = "AI_" + Random.Range(100, 999);
                    tagScript.SetName(aiName);
                    tagScript.SetScore(0);
                }

                AIFollowPlayer aiFollow = ai.GetComponent<AIFollowPlayer>();
                if (aiFollow != null)
                {
                    aiFollow.OnKillCountChanged += tagScript.SetScore;
                }
                aiFollow.nameTag = nameTag;
                GameManager.instance?.RegisterAI();

                yield return null;
            }
        }
    }

    private Vector3 GetValidSpawnPoint(Transform area, float minDistance, int maxAttempts = 20)
    {
        MeshRenderer renderer = area.GetComponent<MeshRenderer>();
        Bounds bounds = renderer.bounds;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidate = new(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            bool isFarEnough = true;
            for (int i = 0; i < usedSpawnPositions.Count; i++)
            {
                if (Vector3.Distance(usedSpawnPositions[i], candidate) < minDistance)
                {
                    isFarEnough = false;
                    break;
                }
            }

            if (isFarEnough)
            {
                usedSpawnPositions.Add(candidate);
                return candidate;
            }
        }

        return new(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public GameObject GetAIFromPool(string prefabName)
    {
        if (!aiPools.TryGetValue(prefabName, out List<GameObject> pool)) return null;

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
                return pool[i];
        }

        return null;
    }

    public void ReturnAI(GameObject ai)
    {
        AIFollowPlayer aiFollow = ai.GetComponent<AIFollowPlayer>();
        if (aiFollow != null && aiFollow.nameTag != null)
        {
            aiFollow.OnKillCountChanged -= aiFollow.nameTag.GetComponent<AINameTag>().SetScore;
            Destroy(aiFollow.nameTag);
            aiFollow.nameTag = null;
        }

        Animator animator = ai.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        ai.SetActive(false);
    }

    public void LaunchHammer(Vector3 start, Vector3 target, float speed = 20f, float time = 0.3f, GameObject owner = null)
    {
        GameObject hammer = GetHammer();
        if (hammer == null) return;

        hammer.transform.position = start;
        hammer.transform.SetParent(null);
        hammer.SetActive(true);

        HammerOwner hammerOwner = hammer.GetComponent<HammerOwner>();
        if (hammerOwner == null)
        {
            hammerOwner = hammer.AddComponent<HammerOwner>();
        }
        hammerOwner.owner = owner ?? gameObject; 

        HammerSpin spin = hammer.GetComponent<HammerSpin>();
        if (spin != null)
        {
            spin.SetOwner(hammerOwner.owner); 
        }

 
        Rigidbody rb = hammer.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 flatTarget = target;
            flatTarget.y = start.y;
            Vector3 direction = (flatTarget - start).normalized;

            rb.AddForce(direction * speed, ForceMode.VelocityChange);
        }

        float flightTime = Vector3.Distance(start, target) / speed;
        StartCoroutine(ReturnAfterDelay(hammer, flightTime + 0.5f));

        Debug.Log($"Launched hammer from {(owner != null ? owner.name : "unknown")}. " +
                  $"Owner tag: {(owner != null ? owner.tag : "no tag")}");
    }

    private IEnumerator ReturnAfterDelay(GameObject hammer, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hammer != null)
        {
            Rigidbody rb = hammer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            HammerOwner hammerOwner = hammer.GetComponent<HammerOwner>();
            if (hammerOwner != null)
            {
                hammerOwner.owner = null; // Reset owner
            }

            hammer.SetActive(false);
        }
    }

    private GameObject GetHammer()
    {
        for (int i = 0; i < hammerPool.Count; i++)
        {
            if (!hammerPool[i].activeInHierarchy)
                return hammerPool[i];
        }

        return null;
    }

}