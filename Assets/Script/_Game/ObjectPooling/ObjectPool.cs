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

    [Header("NameTag Settings")]
    [SerializeField] private GameObject aiNameTagPrefab;
    [SerializeField] private int nameTagPoolSize = 50;

    private readonly Dictionary<string, List<GameObject>> aiPools = new();
    private readonly List<Vector3> usedSpawnPositions = new();
    private readonly List<GameObject> hammerPool = new();
    private readonly List<GameObject> nameTagPool = new();
    private List<int> availableNameNumbers = new List<int>();

    private void Awake()
    {
        Instance = this;
        CreateAllPools();
        InitAvailableNames();
    }

    private void Start()
    {
        StartCoroutine(SpawnAIInAreas());
    }

    #region POOL CREATION
    private void CreateAllPools()
    {
        CreateAIPools();
        CreateHammerPool();
        CreateNameTagPool();
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

    private void CreateNameTagPool()
    {
        for (int i = 0; i < nameTagPoolSize; i++)
        {
            GameObject nameTag = Instantiate(aiNameTagPrefab, transform);
            nameTag.SetActive(false);
            nameTagPool.Add(nameTag);
        }
    }
    #endregion

    #region AI SPAWN
    private IEnumerator SpawnAIInAreas()
    {
        usedSpawnPositions.Clear();

        foreach (Transform area in spawnAreas)
        {
            yield return StartCoroutine(SpawnAIInSingleArea(area));
        }
    }

    private IEnumerator SpawnAIInSingleArea(Transform area)
    {
        for (int i = 0; i < aiPerType; i++)
        {
            GameObject prefab = aiPrefabs[Random.Range(0, aiPrefabs.Count)];
            Vector3 spawnPos = GetValidSpawnPoint(area, 15f);

            GameObject ai = GetAIFromPool(prefab.name);
            if (ai == null) continue;

            SpawnSingleAI(ai, spawnPos);
            yield return null;
        }
    }

    private void SpawnSingleAI(GameObject ai, Vector3 spawnPos)
    {
        ai.transform.position = spawnPos;
        ai.SetActive(true);

        GameObject nameTag = SetupNameTag(ai);
        LinkAIWithNameTag(ai, nameTag);

        GameManager.instance?.RegisterAI();
    }

    private GameObject SetupNameTag(GameObject ai)
    {
        GameObject nameTag = GetNameTagFromPool();
        nameTag.SetActive(true);

        ConfigureNameTagFollowing(nameTag, ai);
        ConfigureNameTagText(nameTag);

        return nameTag;
    }

    private void ConfigureNameTagFollowing(GameObject nameTag, GameObject ai)
    {
        UIManager followHead = nameTag.GetComponent<UIManager>();
        if (followHead != null)
        {
            Transform aiHead = ai.transform.Find("Head") ?? ai.transform;
            followHead.target = aiHead;
            followHead.offset = new Vector3(0, 1.5f, 0);
        }
    }

    private void ConfigureNameTagText(GameObject nameTag)
    {
        AINameTag tagScript = nameTag.GetComponent<AINameTag>();
        if (tagScript != null)
        {
            string aiName = "AI_" + GetRandomUniqueNameNumber();
            tagScript.SetName(aiName);
            tagScript.SetScore(0);
        }
    }

    private void LinkAIWithNameTag(GameObject ai, GameObject nameTag)
    {
        AIFollowPlayer aiFollow = ai.GetComponent<AIFollowPlayer>();
        AINameTag tagScript = nameTag.GetComponent<AINameTag>();

        if (aiFollow != null && tagScript != null)
        {
            aiFollow.OnKillCountChanged += tagScript.SetScore;
            aiFollow.nameTag = nameTag;
        }
    }
    #endregion

    #region SPAWN UTILS
    private Vector3 GetValidSpawnPoint(Transform area, float minDistance, int maxAttempts = 20)
    {
        MeshRenderer renderer = area.GetComponent<MeshRenderer>();
        Bounds bounds = renderer.bounds;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidate = GenerateRandomPositionInBounds(bounds);

            if (IsFarEnough(candidate, minDistance))
            {
                usedSpawnPositions.Add(candidate);
                return candidate;
            }
        }

        return GenerateRandomPositionInBounds(bounds);
    }

    private Vector3 GenerateRandomPositionInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private bool IsFarEnough(Vector3 point, float minDistance)
    {
        foreach (var used in usedSpawnPositions)
        {
            if (Vector3.Distance(used, point) < minDistance)
                return false;
        }
        return true;
    }
    #endregion

    #region AI POOL MANAGEMENT
    public GameObject GetAIFromPool(string prefabName)
    {
        if (!aiPools.TryGetValue(prefabName, out List<GameObject> pool))
            return null;

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                GameObject ai = pool[i];
                ResetAIState(ai);
                return ai;
            }
        }
        return null;
    }

    private void ResetAIState(GameObject ai)
    {
        AIFollowPlayer aiFollow = ai.GetComponent<AIFollowPlayer>();
        if (aiFollow != null)
        {
            aiFollow.killCount = 0;
            ai.transform.localScale = Vector3.one;
            ai.tag = "AI";

            ResetAIComponents(ai);
        }
    }

    private void ResetAIComponents(GameObject ai)
    {
        Collider collider = ai.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        AudioListener listener = ai.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = false;
    }

    public void ReturnAI(GameObject ai)
    {
        UnlinkNameTagFromAI(ai);
        ResetAnimator(ai);
        ai.SetActive(false);
    }

    private void UnlinkNameTagFromAI(GameObject ai)
    {
        AIFollowPlayer aiFollow = ai.GetComponent<AIFollowPlayer>();
        if (aiFollow != null && aiFollow.nameTag != null)
        {
            AINameTag tagScript = aiFollow.nameTag.GetComponent<AINameTag>();
            if (tagScript != null)
            {
                aiFollow.OnKillCountChanged -= tagScript.SetScore;
            }

            ReturnNameTagToPool(aiFollow.nameTag);
            aiFollow.nameTag = null;
        }
    }

    private void ResetAnimator(GameObject ai)
    {
        Animator animator = ai.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
    #endregion

    #region NAMETAG POOL MANAGEMENT
    private GameObject GetNameTagFromPool()
    {
        foreach (GameObject nameTag in nameTagPool)
        {
            if (!nameTag.activeInHierarchy)
            {
                ResetNameTag(nameTag);
                return nameTag;
            }
        }

        return CreateNewNameTag();
    }

    private GameObject CreateNewNameTag()
    {
        GameObject newNameTag = Instantiate(aiNameTagPrefab, transform);
        newNameTag.SetActive(false);
        nameTagPool.Add(newNameTag);
        return newNameTag;
    }

    private void ResetNameTag(GameObject nameTag)
    {
        AINameTag tagScript = nameTag.GetComponent<AINameTag>();
        if (tagScript != null)
        {
            tagScript.SetName("");
            tagScript.SetScore(0);
        }
    }

    private void ReturnNameTagToPool(GameObject nameTag)
    {
        ResetNameTag(nameTag);
        nameTag.SetActive(false);
    }
    #endregion

    #region HAMMER POOL MANAGEMENT
    public void LaunchHammer(Transform spawnPoint, Vector3 target, float speed = 20f, float time = 0.3f, GameObject owner = null)
    {
        GameObject hammer = GetHammer();
        if (hammer == null) return;

        SetupHammer(hammer, spawnPoint, owner);
        LaunchHammerPhysics(hammer, spawnPoint.position, target, speed);

        float flightTime = Vector3.Distance(spawnPoint.position, target) / speed;
        StartCoroutine(ReturnAfterDelay(hammer, flightTime + 0.5f));
    }

    private void SetupHammer(GameObject hammer, Transform spawnPoint, GameObject owner)
    {
        hammer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        hammer.transform.SetParent(null);
        hammer.SetActive(true);

        ConfigureHammerOwnership(hammer, owner);
    }

    private void ConfigureHammerOwnership(GameObject hammer, GameObject owner)
    {
        HammerOwner hammerOwner = hammer.GetComponent<HammerOwner>() ?? hammer.AddComponent<HammerOwner>();
        hammerOwner.owner = owner ?? gameObject;

        HammerSpin spin = hammer.GetComponent<HammerSpin>();
        if (spin != null)
        {
            spin.SetOwner(hammerOwner.owner);
        }
    }

    private void LaunchHammerPhysics(GameObject hammer, Vector3 spawnPos, Vector3 target, float speed)
    {
        Rigidbody rb = hammer.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 direction = CalculateHammerDirection(spawnPos, target);
            rb.AddForce(direction * speed, ForceMode.VelocityChange);
        }
    }

    private Vector3 CalculateHammerDirection(Vector3 spawnPos, Vector3 target)
    {
        Vector3 flatTarget = target;
        flatTarget.y = spawnPos.y;
        return (flatTarget - spawnPos).normalized;
    }

    private IEnumerator ReturnAfterDelay(GameObject hammer, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hammer != null)
        {
            ResetHammerPhysics(hammer);
            hammer.SetActive(false);
        }
    }

    private void ResetHammerPhysics(GameObject hammer)
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
            hammerOwner.owner = null;
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
    #endregion

    #region NAME MANAGEMENT
    private void InitAvailableNames()
    {
        availableNameNumbers.Clear();
        for (int i = 1; i <= 50; i++)
        {
            availableNameNumbers.Add(i);
        }
    }

    private int GetRandomUniqueNameNumber()
    {
        if (availableNameNumbers.Count == 0)
        {
            InitAvailableNames();
        }

        int index = Random.Range(0, availableNameNumbers.Count);
        int number = availableNameNumbers[index];
        availableNameNumbers.RemoveAt(index);
        return number;
    }
    #endregion

    public void RegisterAIScore(string aiName, int score)
    {
        LeaderboardManager.Instance.UpdateScore(aiName, score);
    }

    public void OnAIDefeated(string aiName, int score)
    {
        RegisterAIScore(aiName, score);
    }
}