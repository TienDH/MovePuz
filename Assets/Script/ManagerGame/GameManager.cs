using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject panelWin;
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelTurtorial1;
    [SerializeField] private GameObject panelTurtorial2;
    [SerializeField] private GameObject panelTurtorial3;    

    [SerializeField] private GameObject Turtorial1;
    [SerializeField] private GameObject Turtorial2;
    [SerializeField] private GameObject Turtorial3;

    [SerializeField] private GameObject backgroundButton;
    [SerializeField] private int currentLevel;
    private bool isWin = false;
    private bool isPaused = false;
    private bool isslider = false;
    private LevelManager levelManager;
    private int lastLevelIndex = -1;
    private void Awake()
    {
        levelManager = Object.FindAnyObjectByType<LevelManager>();
        currentLevel = levelManager.GetCurrentLevelIndex() + 1;
    }

    private void Start()
    {
        panelWin.SetActive(false);
        panelPause.SetActive(false);    
        Time.timeScale = 1;
    }

    

    private void Update()
    {
        if (isWin)
        {
            isWin = false;
            Invoke(nameof(SetActiveWin), 1.5f);
        }

        int currentLevel = levelManager.GetCurrentLevelIndex();
        if (currentLevel != lastLevelIndex)
        {
            lastLevelIndex = currentLevel;
            ShowTutorial(currentLevel + 1); // +1 nếu level của bạn hiển thị từ 1
        }
    }

    private void TurnOffTurtorial()
    {
        Turtorial1.SetActive(false);
        panelTurtorial1.SetActive(false);
        Turtorial2.SetActive(false);
        panelTurtorial2.SetActive(false);
        Turtorial3.SetActive(false);
        panelTurtorial3.SetActive(false);
    }
    private void ShowTutorial(int level)
    {
        // Tắt toàn bộ trước
        TurnOffTurtorial();

        // Mở từng tutorial theo level
        switch (level)
        {
            case 1:
                Turtorial1.SetActive(true);
                panelTurtorial1.SetActive(true);
                break;
            case 3:
                Turtorial2.SetActive(true);
                panelTurtorial2.SetActive(true);
                break;
            case 11:
                Turtorial3.SetActive(true);
                panelTurtorial3.SetActive(true);
                break;
        }
    }


    private void SetActiveWin()
    {
        if (panelWin != null)
        {
            panelWin.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("Panel Win activated");
            TurnOffTurtorial();
        }
        else
        {
            Debug.LogError("panelWin is not assigned in Inspector");
        }
    }

    public void WinGame()
    {
        AudioManager.Instance.PlayVFX(AudioManager.Instance.Win);
        isWin = true;
        
    }

    public void NextLevel()
    {
        if (panelWin != null)
        {
            panelWin.SetActive(false);
        }

        Time.timeScale = 1;

        // Cập nhật level đã mở khóa
        int nextLevelIndex = levelManager.GetCurrentLevelIndex() + 1;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (nextLevelIndex + 1 > unlockedLevel) // Cộng thêm 1 vì index bắt đầu từ 0
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevelIndex + 1);
            PlayerPrefs.Save();
            Debug.Log($"Unlocked level {nextLevelIndex + 1}");
        }

        levelManager.LoadNextLevel();
        currentLevel = levelManager.GetCurrentLevelIndex() + 1;
        Debug.Log($"Moved to level {currentLevel}");
    }

    public void TogglePauseUI()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            panelPause.SetActive(true);
            backgroundButton.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            panelPause.SetActive(false);
            backgroundButton.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void TogglesliderUI()
    {
        isslider = !isslider;
    }

    public void CloseSliderUI()
    {
        isslider = false;
        isPaused = false;
        panelPause.SetActive(false);
        backgroundButton.SetActive(false);
        Time.timeScale = 1;
    }

    public void ResumeGame()
    {
        isPaused = false;
        panelPause.SetActive(false);
        backgroundButton.SetActive(false);
        Time.timeScale = 1;
    }

    public void RestartLevel()
    {
        if (panelWin != null)
        {
            panelWin.SetActive(false); // Tắt panelWin khi restart
            Debug.Log("Panel Win deactivated in RestartLevel");
        }
        panelPause.SetActive(false);
        backgroundButton.SetActive(false);
        Time.timeScale = 1;
        levelManager.RestartLevel();
        currentLevel = levelManager.GetCurrentLevelIndex() + 1;
    }
    //public void getTurtorial1()
    //{
    //    Turtorial1.SetActive(true);
    //    panelTurtorial1.SetActive(true);
    //}
}