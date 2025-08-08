using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;

    private void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); // Mặc định chỉ mở Level 1

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i; // Local copy để tránh lỗi delegate
            bool isUnlocked = (i + 1) <= unlockedLevel;
            levelButtons[i].interactable = isUnlocked;

            if (isUnlocked)
            {
                levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }
    }

    public void LoadLevel(int levelIndex)
    {
        PlayerPrefs.SetInt("SelectedLevelIndex", levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Classic"); // Thay bằng tên scene chính của bạn
    }
}