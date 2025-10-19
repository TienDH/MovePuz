using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<LevelConfig> levelConfigs;
    [SerializeField] private MatrixGame matrixGame;
    [SerializeField] private TextMeshProUGUI levelText;
    private int currentLevelIndex = 0;
    private int totalLevelsCompleted = 0;

    void Start()
    {
        Debug.Log("Kiểm tra và khởi tạo level...");

        if (PlayerPrefs.HasKey("SelectedLevelIndex"))
        {
            currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);
            Debug.Log($"Đã chọn level từ Level Select: {currentLevelIndex + 1}");
        }

        ValidateAndInitializeLevel();
        
    }

    private void ValidateAndInitializeLevel()
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogError("Danh sách levelConfigs rỗng hoặc không được gán trong LevelManager!");
            matrixGame?.SetLevelConfig(null);
            UpdateLevelText(null);
            return;
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelConfigs.Count - 1);
        Debug.Log($"Bắt đầu với level {currentLevelIndex + 1}: {levelConfigs[currentLevelIndex].name}");

        if (matrixGame != null)
        {
            LoadLevel(currentLevelIndex);
        }
    }

    public void LoadNextLevel()
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogError("Danh sách levelConfigs rỗng! Không thể tải level tiếp theo.");
            UpdateLevelText(null);
            return;
        }
        currentLevelIndex++;
        totalLevelsCompleted++;
        if (currentLevelIndex >= levelConfigs.Count)
        {
            currentLevelIndex = 0;
            Debug.Log("Đã hoàn thành tất cả các level! Bắt đầu lại từ level đầu tiên.");
        }
        LoadLevel(currentLevelIndex);
        Debug.Log($"Đã tải level tiếp theo {currentLevelIndex + 1}: {levelConfigs[currentLevelIndex].name}");

        Debug.Log("Tổng số màn đã hoàn thành: " + totalLevelsCompleted);

    }

    public void LoadLevel(int index)
    {
        Debug.Log($"Đang cố gắng tải level với index {index}");
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogError($"Danh sách levelConfigs rỗng hoặc không được gán trong LevelManager!");
            if (matrixGame != null)
            {
                matrixGame.SetLevelConfig(null);
            }
            UpdateLevelText(null);
            return;
        }
        if (index < 0 || index >= levelConfigs.Count)
        {
            Debug.LogError($"Chỉ số level không hợp lệ {index}. Số lượng level: {levelConfigs.Count}. Đặt lại về 0.");
            currentLevelIndex = 0;
        }
        else
        {
            currentLevelIndex = index;
        }

        if (matrixGame == null)
        {
            Debug.LogError("MatrixGame không được gán trong LevelManager!");
            return;
        }

        Debug.Log($"Đang tải level {currentLevelIndex + 1} với cấu hình: {levelConfigs[currentLevelIndex].name}");
        matrixGame.SetLevelConfig(levelConfigs[currentLevelIndex]);
        UpdateLevelText(levelConfigs[currentLevelIndex]);
    }

    public void RestartLevel()
    {
        Debug.Log("Bắt đầu reset level...");
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogError("Danh sách levelConfigs rỗng! Không thể reset level.");
            UpdateLevelText(null);
            return;
        }
        LoadLevel(currentLevelIndex);
        Debug.Log($"Đã reset level {currentLevelIndex + 1}: {levelConfigs[currentLevelIndex].name}");
    }
    public void OnClick_NextFromWin()
    {
        AdsManager instance = FindObjectOfType<AdsManager>(); // hoặc AdsManager.Instance nếu bạn có property Instance
        instance?.OnLevelCompleted_SafeTryShow();

        // 2) Sang màn tiếp theo
        LoadNextLevel();
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public LevelConfig GetCurrentLevelConfig()
    {
        if (levelConfigs == null || levelConfigs.Count == 0 || currentLevelIndex < 0 || currentLevelIndex >= levelConfigs.Count)
        {
            Debug.LogWarning("Không thể lấy cấu hình level hiện tại do levelConfigs rỗng hoặc chỉ số không hợp lệ.");
            return null;
        }
        return levelConfigs[currentLevelIndex];
    }

    private void UpdateLevelText(LevelConfig currentLevel)
    {
        if (levelText == null)
        {
            Debug.LogError("TextMeshProUGUI chưa được gán trong LevelManager!");
            return;
        }

        if (currentLevel != null)
        {
            levelText.text = $"Level {currentLevelIndex + 1}";
        }
        else
        {
            levelText.text = "Không có level";
        }
    }
}