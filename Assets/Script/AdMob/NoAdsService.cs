using UnityEngine;
using System;

public class NoAdsService : MonoBehaviour
{
    public static NoAdsService Instance { get; private set; }

    public const string PP_NO_ADS = "no_ads"; // 0/1
    public bool IsNoAds { get; private set; }

    public event Action<bool> OnNoAdsChanged; // (isNoAds)

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsNoAds = PlayerPrefs.GetInt(PP_NO_ADS, 0) == 1;
    }

    public void MarkNoAdsPurchased()
    {
        if (IsNoAds) return;
        IsNoAds = true;
        PlayerPrefs.SetInt(PP_NO_ADS, 1);
        PlayerPrefs.Save();
        OnNoAdsChanged?.Invoke(true);
        Debug.Log("[NoAds] Marked purchased.");
    }

    // Hỗ trợ QA
    [ContextMenu("Reset NoAds")]
    public void DebugReset()
    {
        IsNoAds = false;
        PlayerPrefs.DeleteKey(PP_NO_ADS);
        PlayerPrefs.Save();
        OnNoAdsChanged?.Invoke(false);
        Debug.Log("[NoAds] Reset.");
    }
}
