using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [Header("Settings")]
    public float startTime = 60f;

    [Header("UI")]
    public TMP_Text timerText;

    private float currentTime;
    private bool isCounting = false;

    private void Start()
    {
        ResetCountdown();
        StartCountdown();
    }

    private void Update()
    {
        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isCounting = false;
            OnTimerEnd();
            return;
        }

        UpdateTimerUI();
    }

    public void StartCountdown() => isCounting = true;

    public void StopCountdown() => isCounting = false;

    public void ResetCountdown()
    {
        currentTime = startTime;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format(GameConstants.TIMER_FORMAT, minutes, seconds);
    }

    private void OnTimerEnd()
    {
        SceneManager.LoadScene(GameConstants.SCENE_LOSE);
    }
}