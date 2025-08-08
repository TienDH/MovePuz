using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider vfxSlider;

    private void Awake()
    {
        // Tìm Slider nếu chưa được gán trong Inspector
        if (musicSlider == null)
        {
            musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
            if (musicSlider == null) Debug.LogWarning("MusicSlider not found in scene");
        }
        if (vfxSlider == null)
        {
            vfxSlider = GameObject.Find("VFXSlider")?.GetComponent<Slider>();
            if (vfxSlider == null) Debug.LogWarning("VFXSlider not found in scene");
        }
    }

    private void Start()
    {
        SetupSliders();
    }

    private void OnEnable()
    {
        // Gán lại Slider khi scene tải
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Gán lại Slider khi scene mới được tải
        SetupSliders();
    }

    private void SetupSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (vfxSlider != null)
        {
            vfxSlider.value = PlayerPrefs.GetFloat("VFXVolume", 1f);
            vfxSlider.onValueChanged.RemoveAllListeners();
            vfxSlider.onValueChanged.AddListener(SetVFXVolume);
        }
    }

    private void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null in UIManager");
        }
    }

    private void SetVFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVFXVolume(value);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null in UIManager");
        }
    }
}