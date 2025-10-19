using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private string gameplaySceneName = "Classic";
    bool _loading;

    void Start()
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int li = i;
            var btn = levelButtons[i];
            bool isUnlocked = (i + 1) <= unlocked;
            btn.interactable = isUnlocked;
            btn.onClick.RemoveAllListeners();
            if (isUnlocked) btn.onClick.AddListener(() => OnLevel(li));
        }
    }

    void OnLevel(int levelIndex)
    {
        if (_loading) return;
        _loading = true;
        foreach (var b in levelButtons) b.interactable = false;

        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        PlayerPrefs.Save();

        LoadingSceneController.LoadScene(gameplaySceneName);
    }
}
