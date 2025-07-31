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

    private Dictionary<string, List<GameObject>> aiPools = new Dictionary<string, List<GameObject>>();
    private List<GameObject> hammerPool = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        foreach (GameObject prefab in aiPrefabs)
        {
            var pool = new List<GameObject>();
            for (int i = 0; i < aiPerType; i++)
            {
                GameObject ai = Instantiate(prefab, transform);
                ai.SetActive(false);
                pool.Add(ai);
            }
            aiPools[prefab.name] = pool;
        }
        for (int i = 0; i < hammerPoolSize; i++)
        {
            GameObject hammer = Instantiate(hammerPrefab, transform);
            hammer.SetActive(false);
            hammerPool.Add(hammer);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnAIInAreas());
    }

    private IEnumerator SpawnAIInAreas()
    {
        foreach (Transform area in spawnAreas)
        {
            for (int i = 0; i < aiPerType; i++)
            {
                GameObject prefab = aiPrefabs[Random.Range(0, aiPrefabs.Count)];
                Vector3 pos = GetRandomPointInArea(area);

                GameObject ai = GetAIFromPool(prefab.name);
                if (ai != null)
                {
                    ai.transform.position = pos;
                    ai.SetActive(true);
                }

                yield return null;
            }
        }
    }

    private Vector3 GetRandomPointInArea(Transform area)
    {
        MeshRenderer r = area.GetComponent<MeshRenderer>();
        Bounds b = r.bounds;
        return new Vector3(Random.Range(b.min.x, b.max.x), b.max.y, Random.Range(b.min.z, b.max.z));
    }

    public GameObject GetAIFromPool(string prefabName)
    {
        if (aiPools.ContainsKey(prefabName))
        {
            foreach (var ai in aiPools[prefabName])
            {
                if (!ai.activeInHierarchy)
                    return ai;
            }
        }
        return null;
    }

    public void ReturnAI(GameObject ai)
    {

        Animator anim = ai.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind(); 
            anim.Update(0f); 
        }
        ai.SetActive(false);
    }

    public void LaunchHammer(Vector3 start, Vector3 target, float speed = 20f, float time = 0.1f)
    {
        GameObject hammer = GetHammer();
        if (hammer == null)
        {
            return;
        }

        hammer.transform.position = start;
        hammer.SetActive(true);
        hammer.transform.SetParent(null);
        Rigidbody rb = hammer.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;

        Vector3 flatTarget = target;
        flatTarget.y = start.y;
        Vector3 direction = (flatTarget - start).normalized;

        rb.AddForce(direction * speed, ForceMode.VelocityChange);

        Debug.DrawRay(start, direction * 5f, Color.yellow, 2f);

        StartCoroutine(ReturnAfterDelay(hammer, time));
    }


    private GameObject GetHammer()
    {
        foreach (var hammer in hammerPool)
        {
            if (!hammer.activeInHierarchy)
            {
                return hammer;
            }
        }
        return null;

    }

    private IEnumerator ReturnAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

}
