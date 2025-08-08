using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeManager : MonoBehaviour
{
    [Header("UI Liên Kết")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject lockIcon;

    [Header("Thiết lập chế độ")]
    [SerializeField] private string modeKey = "Classic";
    [SerializeField] private Button modeButton;
    [SerializeField] private int totalLevels = 20;

    [Header("Chế độ tiếp theo")]
    [SerializeField] private GameObject nextModeObject;

    private const string LEVEL_KEY = "LevelUnlocked_";
    private const string STAR_KEY = "LevelStars_";

    private void Start()
    {
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        if (progressText == null || progressBar == null || lockIcon == null || modeButton == null)
        {
            Debug.LogError("Thiếu liên kết UI trên " + gameObject.name);
            return;
        }

        int completedLevels = CountCompletedLevels();
        progressText.text = completedLevels + "/" + totalLevels;

        float fillAmount = Mathf.Clamp01((float)completedLevels / totalLevels);
        progressBar.fillAmount = fillAmount;

        if (completedLevels >= totalLevels)
        {
            UnlockNextMode();
        }
        else if (nextModeObject != null)
        {
            ModeManager nextMode = nextModeObject.GetComponent<ModeManager>();
            if (nextMode != null)
            {
                nextMode.SetLocked(true);
            }
        }

        bool isUnlocked = IsModeUnlocked();
        modeButton.interactable = isUnlocked;
        lockIcon.SetActive(!isUnlocked);
    }

    private int CountCompletedLevels()
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++)
        {
            bool isUnlocked = PlayerPrefs.GetInt(LEVEL_KEY + modeKey + "_" + i, 0) == 1;
            if (isUnlocked)
                count++;
        }
        return count;
    }

    private bool IsModeUnlocked()
    {
        if (modeKey == "Classic")
            return true;

        return PlayerPrefs.GetInt("ModeUnlocked_" + modeKey, 0) == 1;
    }

    private void UnlockNextMode()
    {
        if (nextModeObject == null)
        {
            Debug.Log("Đã hoàn thành toàn bộ chế độ: " + modeKey);
            return;
        }

        ModeManager nextMode = nextModeObject.GetComponent<ModeManager>();
        if (nextMode != null)
        {
            PlayerPrefs.SetInt("ModeUnlocked_" + nextMode.modeKey, 1);
            PlayerPrefs.Save();
            nextMode.SetLocked(false);
        }
    }

    public void SetLocked(bool locked)
    {
        modeButton.interactable = !locked;
        lockIcon.SetActive(locked);
    }
}
