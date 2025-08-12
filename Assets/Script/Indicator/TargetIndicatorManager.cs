using System.Collections.Generic;
using UnityEngine;

public class TargetIndicatorManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxIndicators = GameConstants.MAX_INDICATORS;
    [SerializeField] private float indicatorEdgePadding = GameConstants.EDGE_PADDING;
    [SerializeField] private GameObject[] indicatorPrefabs;

    // Cache components and collections
    private List<AIFollowPlayer> allAIs = new List<AIFollowPlayer>();
    private List<TargetIndicator> activeIndicators = new List<TargetIndicator>();
    private Camera cachedMainCamera;
    private Transform cachedCameraTransform;

    private void Start()
    {
        // Cache camera
        cachedMainCamera = Camera.main;
        cachedCameraTransform = cachedMainCamera.transform;

        InitializeAIList();
        InitializeIndicatorPool();
    }

    private void InitializeAIList()
    {
        AIFollowPlayer[] foundAIs = FindObjectsOfType<AIFollowPlayer>();
        for (int i = 0; i < foundAIs.Length; i++)
        {
            allAIs.Add(foundAIs[i]);
        }
    }

    private void InitializeIndicatorPool()
    {
        for (int i = 0; i < maxIndicators; i++)
        {
            GameObject prefabToUse = indicatorPrefabs[i % indicatorPrefabs.Length];
            GameObject indicator = Instantiate(prefabToUse, transform);
            indicator.SetActive(false);

            TargetIndicator targetIndicator = indicator.GetComponent<TargetIndicator>();
            activeIndicators.Add(targetIndicator);
        }
    }

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        List<AIFollowPlayer> aliveAIs = GetAliveAIs();
        List<AIFollowPlayer> nearestAIs = GetNearestAIs(aliveAIs);

        for (int i = 0; i < maxIndicators; i++)
        {
            Transform targetTransform = i < nearestAIs.Count ? nearestAIs[i].transform : null;
            activeIndicators[i].UpdateTarget(targetTransform);
        }
    }

    private List<AIFollowPlayer> GetAliveAIs()
    {
        List<AIFollowPlayer> aliveAIs = new List<AIFollowPlayer>();
        for (int i = 0; i < allAIs.Count; i++)
        {
            if (!allAIs[i].IsDead())
            {
                aliveAIs.Add(allAIs[i]);
            }
        }
        return aliveAIs;
    }

    private List<AIFollowPlayer> GetNearestAIs(List<AIFollowPlayer> aliveAIs)
    {
        // Sort by distance using simple bubble sort (Junior level)
        for (int i = 0; i < aliveAIs.Count - 1; i++)
        {
            for (int j = 0; j < aliveAIs.Count - i - 1; j++)
            {
                float distanceJ = Vector3.Distance(cachedCameraTransform.position, aliveAIs[j].transform.position);
                float distanceJ1 = Vector3.Distance(cachedCameraTransform.position, aliveAIs[j + 1].transform.position);

                if (distanceJ > distanceJ1)
                {
                    AIFollowPlayer temp = aliveAIs[j];
                    aliveAIs[j] = aliveAIs[j + 1];
                    aliveAIs[j + 1] = temp;
                }
            }
        }

        // Take only the nearest ones
        List<AIFollowPlayer> nearestAIs = new List<AIFollowPlayer>();
        int count = Mathf.Min(maxIndicators, aliveAIs.Count);
        for (int i = 0; i < count; i++)
        {
            nearestAIs.Add(aliveAIs[i]);
        }

        return nearestAIs;
    }
}