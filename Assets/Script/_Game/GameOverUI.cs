using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float fadeAlpha = 0.7f;

    private void Awake()
    {
        if (fadeOverlay != null)
            fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.g, fadeOverlay.color.b, 0);

        if (restartButton != null)
            restartButton.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.DOFade(fadeAlpha, fadeDuration).OnComplete(() =>
            {
                if (restartButton != null)
                    restartButton.SetActive(true);
            });
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
