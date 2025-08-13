using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance { get; private set; }

    [Header("UI Follow Settings")]
    public Transform target;
    public Vector3 offset;

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 60f;
    [SerializeField] private TMP_Text timerText;

    [Header("Leaderboard UI")]
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderBoardPanel;

    [Header("Target Indicators")]
    [SerializeField] private int maxIndicators = 5;
    [SerializeField] private float indicatorEdgePadding = 50f;
    [SerializeField] private GameObject[] indicatorPrefabs;

    [Header("Level Navigation")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private TMP_Text textLose;
    [SerializeField] private TMP_Text textWin;

    [Header("UI Follow Settings")]
    [SerializeField] private Transform uiFollowTarget;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.2f, 0);

    [Header("Level Selection Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    // Private variables
    private float currentTime;
    private bool isCounting = false;
    public event Action OnTimeUp;

    private List<TargetIndicator> activeIndicators = new List<TargetIndicator>();
    private List<AIFollowPlayer> allAIs = new List<AIFollowPlayer>();
    private Camera mainCam;
    private Transform camTransform;

    #region Unity Lifecycle
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void RefreshLeaderboardUI(List<LeaderboardData> entries)
    {
        // Clear current leaderboard
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Display top entries
        int count = Mathf.Min(5, entries.Count);
        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            var entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            var texts = entryObj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = entry.playerName;
            texts[1].text = entry.playerScore.ToString();
        }
    }
    private void Start()
    {
        InitializeLevelButtons();
        InitializeTimer();
        InitializeLeaderboard();
        InitializeIndicators();
        InitializeButtons();
        CacheCamera();
    }

    private void Update()
    {
        UpdateTimer();
        UpdateIndicators();
        UpdateUIPosition();
    }
    #endregion

    #region Timer System
    private void InitializeTimer()
    {
        currentTime = startTime;
        UpdateTimerDisplay();
        StartTimer();
    }

    private void UpdateTimer()
    {
        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            StopTimer();
            OnTimerEnd();
            return;
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer() => isCounting = true;
    public void StopTimer() => isCounting = false;
    public void ResetTimer()
    {
        currentTime = startTime;
        UpdateTimerDisplay();
    }

    private void OnTimerEnd()
    {
        SceneManager.LoadScene("LoseScene");
    }
    #endregion

    #region Leaderboard System
    private void InitializeLeaderboard()
    {
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void RefreshLeaderboard(List<LeaderboardEntry> entries)
    {
        // Clear current leaderboard
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Display top 5 entries
        int count = Mathf.Min(5, entries.Count);
        for (int i = 0; i < count; i++)
        {
            var entry = entries[i];
            var entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            var texts = entryObj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = entry.name;
            texts[1].text = entry.score.ToString();
        }
    }
    #endregion

    #region Target Indicator System
    private void CacheCamera()
    {
        mainCam = Camera.main;
        camTransform = mainCam.transform;
    }

    private void InitializeIndicators()
    {
        // Create indicator pool
        for (int i = 0; i < maxIndicators; i++)
        {
            var prefab = indicatorPrefabs[i % indicatorPrefabs.Length];
            var indicator = Instantiate(prefab, transform).GetComponent<TargetIndicator>();
            indicator.gameObject.SetActive(false);
            activeIndicators.Add(indicator);
        }

        // Find all AI enemies
        var foundAIs = FindObjectsOfType<AIFollowPlayer>();
        allAIs.AddRange(foundAIs);
    }

    private void UpdateIndicators()
    {
        var aliveAIs = GetAliveAIs();
        var nearestAIs = GetNearestAIs(aliveAIs);

        // Update each indicator with target
        for (int i = 0; i < maxIndicators; i++)
        {
            var target = i < nearestAIs.Count ? nearestAIs[i].transform : null;
            activeIndicators[i].UpdateTarget(target);
        }
    }

    private List<AIFollowPlayer> GetAliveAIs()
    {
        return allAIs.FindAll(ai => !ai.IsDead());
    }

    private List<AIFollowPlayer> GetNearestAIs(List<AIFollowPlayer> aIs)
    {
        // Simple distance-based sorting
        aIs.Sort((a, b) =>
            Vector3.Distance(camTransform.position, a.transform.position)
            .CompareTo(Vector3.Distance(camTransform.position, b.transform.position)));

        // Return only needed count
        return aIs.GetRange(0, Mathf.Min(maxIndicators, aIs.Count));
    }
    #endregion

    #region Level Navigation
    private void InitializeButtons()
    {
        if (nextLevelButton) nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (restartButton) restartButton.onClick.AddListener(RestartLevel);
        if (menuButton) menuButton.onClick.AddListener(ReturnToMenu);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene("NextLevelScene");
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region UI Positioning
    private void UpdateUIPosition()
    {
        if (uiFollowTarget == null) return;

        // Update position with offset
        transform.position = uiFollowTarget.position + uiOffset;

        // Face toward camera (but keep upright)
        Vector3 lookDirection = mainCam.transform.forward;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
    #endregion

    #region Level Selection System
    private void InitializeLevelButtons()
    {
        if (level1Button != null)
        {
            level1Button.onClick.AddListener(LoadLevel1);
        }

        if (level2Button != null)
        {
            level2Button.onClick.AddListener(LoadLevel2);
        }
    }

    private void LoadLevel1()
    {
        SceneManager.LoadScene("SceneReal");
    }

    private void LoadLevel2()
    {
        SceneManager.LoadScene("SceneReal 1");
    }
    #endregion
}

// Supporting data structures
[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int score;
}
