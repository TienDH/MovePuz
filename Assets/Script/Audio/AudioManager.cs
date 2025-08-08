using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource vfxSource;

    [SerializeField] public AudioClip music;
    [SerializeField] public AudioClip MoveBlock;
    [SerializeField] public AudioClip Win;
    [SerializeField] public AudioClip clip;
    [SerializeField] public AudioClip jump;
    [SerializeField] public AudioClip click;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        // Kiểm tra và khởi tạo AudioSource
        if (musicSource == null)
        {
            Debug.LogWarning("musicSource is not assigned in AudioManager");
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (vfxSource == null)
        {
            Debug.LogWarning("vfxSource is not assigned in AudioManager");
            vfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Kiểm tra AudioClip
        if (music == null) Debug.LogWarning("music AudioClip is not assigned");
        if (click == null) Debug.LogWarning("click AudioClip is not assigned");

        // Khởi tạo musicSource
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        // Khởi tạo vfxSource
        vfxSource.volume = PlayerPrefs.GetFloat("VFXVolume", 1f);
    }

    public void PlayVFX(AudioClip clip)
    {
        if (vfxSource != null && clip != null)
        {
            vfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Cannot play VFX: vfxSource={(vfxSource == null ? "null" : "set")}, clip={(clip == null ? "null" : clip.name)}");
        }
    }

    public void ClickButton()
    {
        if (click != null)
        {
            PlayVFX(click);
        }
        else
        {
            Debug.LogWarning("click AudioClip is not assigned in AudioManager");
        }
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
        {
            musicSource.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    public void SetVFXVolume(float value)
    {
        if (vfxSource != null)
        {
            vfxSource.volume = value;
            PlayerPrefs.SetFloat("VFXVolume", value);
        }
    }
}