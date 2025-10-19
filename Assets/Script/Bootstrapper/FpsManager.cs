using UnityEngine;

/// Quản lý FPS/v-sync tập trung. Gọi ở runtime: SetMenuMode(), SetGameplayMode(), SetBatterySaver(), SetCustomCap().
public class FpsManager : MonoBehaviour
{
    public static FpsManager Instance { get; private set; }

    // Các mốc cap phổ biến
    [Header("Caps")]
    [SerializeField] int menuFps = 30;
    [SerializeField] int gameplayFps = 60;
    [SerializeField] int batterySaverFps = 30;

    [Header("Options")]
    [SerializeField] bool adaptiveToDisplay = false;   // nếu true: cap không vượt refresh rate màn

    int _currentCap;

    // ==== Singleton ====
    public static void EnsureInstance()
    {
        if (Instance) return;
        var go = new GameObject("[FpsManager]");
        Instance = go.AddComponent<FpsManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Apply(gameplayFps); // mặc định
    }

    // ==== Public API ====
    public void SetMenuMode() => Apply(menuFps);
    public void SetGameplayMode() => Apply(gameplayFps);
    public void SetBatterySaver() => Apply(batterySaverFps);
    public void SetCustomCap(int fps) => Apply(Mathf.Max(15, fps)); // bảo vệ cap quá thấp
    public void SetAdaptive(bool on) { adaptiveToDisplay = on; Apply(_currentCap); }

    // ==== Core ====
    void Apply(int desiredFps)
    {
        _currentCap = desiredFps;
        int displayHz = GetDisplayHz();

        // Giới hạn theo màn khi adaptive bật (tránh cố 120 trên màn 60)
        int capped = adaptiveToDisplay ? Mathf.Min(desiredFps, displayHz) : desiredFps;

        QualitySettings.vSyncCount = 1;          // đồng bộ theo màn hình (ổn định/pin tốt)
        Application.targetFrameRate = capped;    // vẫn cap để hạn chế CPU/GPU khi v-sync off tạm thời

        // Tuỳ chọn: log nhẹ để kiểm tra
        // Debug.Log($"[FpsManager] Apply -> desired:{desiredFps}, displayHz:{displayHz}, set:{capped}");
    }

    // Lấy tần số làm tươi của thiết bị (Unity mới có RefreshRateRatio)
    int GetDisplayHz()
    {
#if UNITY_2022_2_OR_NEWER
        // value là double → ép float trước khi RoundToInt
        var rr = Screen.currentResolution.refreshRateRatio;
        return Mathf.RoundToInt((float)rr.value);
#elif UNITY_2021_2_OR_NEWER
    // 2021 có refreshRate (int)
    return Screen.currentResolution.refreshRate;
#else
    return 60;
#endif
    }
}
