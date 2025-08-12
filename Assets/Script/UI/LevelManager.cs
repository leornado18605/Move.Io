using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button endGameButton;

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (endGameButton != null)
            endGameButton.onClick.AddListener(EndGame);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(GameConstants.SCENE_REAL_1);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(GameConstants.SCENE_REAL);
    }

    public void EndGame()
    {
        SceneManager.LoadScene(GameConstants.SCENE_MENU);
    }

    public void NextLevel1()
    {
        SceneManager.LoadScene(GameConstants.SCENE_REAL);
    }
}