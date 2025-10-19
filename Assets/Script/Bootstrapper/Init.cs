using UnityEngine;


/// Chạy trước mọi scene để cấu hình FPS, v-sync, GC… cực sớm.
public static class Init
{
    // Tham số mặc định – có thể chỉnh trong FpsManager sau này.
    const int DEFAULT_GAMEPLAY_FPS = 60;
    const int DEFAULT_MENU_FPS = 30;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        // Android tối ưu an toàn
#if UNITY_ANDROID
        QualitySettings.vSyncCount = 1;                 // đồng bộ theo tần số màn hình
        Application.targetFrameRate = DEFAULT_GAMEPLAY_FPS;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.BelowNormal;
#else
        // Editor/PC: giữ vsync, cap high hơn để debug
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = DEFAULT_GAMEPLAY_FPS;
#endif

        // Tạo FpsManager (DontDestroyOnLoad) + set gameplay mặc định
        FpsManager.EnsureInstance();
        FpsManager.Instance.SetGameplayMode();          // vào game mặc định 60
        // Nếu scene đầu tiên là Menu, bạn gọi SetMenuMode() từ controller của Menu.
    }
}
