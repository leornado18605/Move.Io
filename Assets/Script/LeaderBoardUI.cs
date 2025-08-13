using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Transform entryContainer;

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        var list = LeaderboardManager.Instance.GetLeaderboard();
        int count = Mathf.Min(5, list.Count);
        for (int i = 0; i < count; i++)
        {
            var entry = list[i];
            GameObject obj = Instantiate(entryPrefab, entryContainer);
            TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>();
            texts[0].text = entry.name;
            texts[1].text = entry.score.ToString();
        }
    }
}
