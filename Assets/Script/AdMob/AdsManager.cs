using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    [Header("AdMob Test Ad Unit IDs (Android)")]
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-2465944213278280/3472332029";
    [SerializeField] private string bannerAdUnitId = "ca-app-pub-2465944213278280/3573432045";

    [Header("Policy")]
    [SerializeField] private int firstAdAfterLevels = 3;
    [SerializeField] private int adFrequencyLevels = 4;
    [SerializeField] private int minSecondsBetweenAds = 45;

    [Header("Diagnostics")]
    [SerializeField] private bool verboseLog = true;

    private InterstitialAd _interstitial;
    private BannerView _banner;

    // Persisted counters
    private const string PP_TOTAL_COMPLETED = "ads_total_completed";
    private const string PP_LEVELS_SINCE = "ads_levels_since";
    private const string PP_LAST_AD_UNIX = "ads_last_ad_unix";

    private int _totalCompleted;
    private int _levelsSinceLastAd;
    private long _lastAdUnix;

    private bool _eligibleButInterstitialNotReady;
    private bool _adsDisabled; // khi No Ads

    void Awake()
    {
        _totalCompleted = PlayerPrefs.GetInt(PP_TOTAL_COMPLETED, 0);
        _levelsSinceLastAd = PlayerPrefs.GetInt(PP_LEVELS_SINCE, 0);
        _lastAdUnix = long.Parse(PlayerPrefs.GetString(PP_LAST_AD_UNIX, "0"));
    }

    void OnEnable()
    {
        if (NoAdsService.Instance != null)
            NoAdsService.Instance.OnNoAdsChanged += HandleNoAdsChanged;
    }

    void OnDisable()
    {
        if (NoAdsService.Instance != null)
            NoAdsService.Instance.OnNoAdsChanged -= HandleNoAdsChanged;
    }

    void Start()
    {
        _adsDisabled = (NoAdsService.Instance && NoAdsService.Instance.IsNoAds);

        if (_adsDisabled)
        {
            Log("[Ads] Disabled by NoAds.");
            return;
        }

        MobileAds.Initialize(_ =>
        {
            Log("AdMob Initialized (Android)");
            PreloadInterstitial();
            // (Banner: bạn tự quyết định lúc nào gọi Request/Show)
        });
    }

    private void HandleNoAdsChanged(bool isNoAds)
    {
        _adsDisabled = isNoAds;
        if (isNoAds)
        {
            Log("NoAds purchased → destroying ads & stop loading.");
            SafeDestroyBanner();
            SafeDestroyInterstitial();
        }
        else
        {
            // Trường hợp bạn cho phép “mở lại” (hiếm khi dùng)
            MobileAds.Initialize(_ => { PreloadInterstitial(); });
        }
    }

    // ===== Public API =====
    public void OnLevelCompleted_SafeTryShow()
    {
        if (_adsDisabled) { Log("NO ADS active → skip ads."); return; }

        _totalCompleted++;
        _levelsSinceLastAd++;
        SaveCounters();

        Log($"[Complete] total={_totalCompleted}, sinceLast={_levelsSinceLastAd}, lastAdAgo={SecondsSinceLastAd()}s");

        if (!IsPolicyEligibleNow())
        {
            _eligibleButInterstitialNotReady = false;
            Log("Not eligible by policy yet.");
            return;
        }

        if (IsInterstitialReady())
        {
            ShowInterstitialInternal();
            _eligibleButInterstitialNotReady = false;
        }
        else
        {
            _eligibleButInterstitialNotReady = true;
            Log("Eligible but interstitial not ready → defer. Preloading...");
            PreloadInterstitial();
        }
    }

    public void TryShowIfDeferred()
    {
        if (_adsDisabled) return;

        if (_eligibleButInterstitialNotReady && IsPolicyEligibleNow() && IsInterstitialReady())
        {
            Log("Deferred eligible → now ready. Showing interstitial.");
            ShowInterstitialInternal();
            _eligibleButInterstitialNotReady = false;
        }
    }

    public void DebugResetAdCounters()
    {
        _totalCompleted = 0;
        _levelsSinceLastAd = 0;
        _lastAdUnix = 0;
        _eligibleButInterstitialNotReady = false;
        SaveCounters();
        Log("Counters reset.");
    }

    // ===== Policy =====
    private bool IsPolicyEligibleNow()
    {
        if (_totalCompleted < Mathf.Max(0, firstAdAfterLevels)) return false;
        if (_levelsSinceLastAd < Mathf.Max(1, adFrequencyLevels)) return false;
        if (SecondsSinceLastAd() < Mathf.Max(0, minSecondsBetweenAds)) return false;
        return true;
    }

    private long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private int SecondsSinceLastAd()
    {
        if (_lastAdUnix <= 0) return int.MaxValue;
        long diff = NowUnix() - _lastAdUnix;
        if (diff < 0) diff = 0;
        return (int)diff;
    }

    private void MarkAdJustShown()
    {
        _levelsSinceLastAd = 0;
        _lastAdUnix = NowUnix();
        SaveCounters();
    }

    private void SaveCounters()
    {
        PlayerPrefs.SetInt(PP_TOTAL_COMPLETED, _totalCompleted);
        PlayerPrefs.SetInt(PP_LEVELS_SINCE, _levelsSinceLastAd);
        PlayerPrefs.SetString(PP_LAST_AD_UNIX, _lastAdUnix.ToString());
        PlayerPrefs.Save();
    }

    // ===== Interstitial =====
    private void PreloadInterstitial()
    {
        if (_adsDisabled) return;

        if (string.IsNullOrEmpty(interstitialAdUnitId))
        {
            LogError("Interstitial Ad Unit ID is empty.");
            return;
        }

        SafeDestroyInterstitial();

        var request = new AdRequest();
        InterstitialAd.Load(interstitialAdUnitId, request, (ad, error) =>
        {
            if (_adsDisabled) { ad?.Destroy(); return; } // vừa mua xong trong lúc load
            if (error != null || ad == null)
            {
                LogError("Interstitial load failed: " + (error == null ? "unknown" : error.GetMessage()));
                return;
            }

            _interstitial = ad;
            Log("Interstitial loaded.");

            _interstitial.OnAdFullScreenContentOpened += () => Log("Interstitial opened.");
            _interstitial.OnAdFullScreenContentClosed += () =>
            {
                Log("Interstitial closed. Preloading next...");
                PreloadInterstitial();
            };
            _interstitial.OnAdFullScreenContentFailed += adError =>
            {
                LogError("Interstitial failed to show: " + (adError == null ? "unknown" : adError.GetMessage()));
                PreloadInterstitial();
            };
        });
    }

    private bool IsInterstitialReady() => _interstitial != null && _interstitial.CanShowAd();

    private void ShowInterstitialInternal()
    {
        if (_adsDisabled) return;
        if (!IsInterstitialReady())
        {
            Log("Show called but interstitial not ready.");
            return;
        }

        Log("Showing interstitial...");
        MarkAdJustShown();

        try
        {
            _interstitial.Show();
        }
        catch (Exception e)
        {
            LogError("Exception when showing interstitial: " + e.Message);
            _levelsSinceLastAd = Mathf.Max(0, _levelsSinceLastAd - 1); // trả lại 1 lần
            SaveCounters();
            PreloadInterstitial();
        }
    }

    // ===== Banner (tùy chọn) =====
    public void RequestBannerAd()
    {
        if (_adsDisabled) { Log("NO ADS active → skip banner."); return; }
        if (string.IsNullOrEmpty(bannerAdUnitId))
        {
            LogError("Banner Ad Unit ID is empty.");
            return;
        }
        SafeDestroyBanner();

        _banner = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        _banner.LoadAd(new AdRequest());
        Log("Banner requested.");
    }

    public void ShowBanner()
    {
        if (_adsDisabled) return;
        _banner?.Show();
        Log("Banner shown.");
    }

    public void HideBanner()
    {
        _banner?.Hide();
        Log("Banner hidden.");
    }

    private void SafeDestroyBanner()
    {
        if (_banner != null)
        {
            try { _banner.Destroy(); } catch { }
            _banner = null;
        }
    }

    private void SafeDestroyInterstitial()
    {
        if (_interstitial != null)
        {
            try { _interstitial.Destroy(); } catch { }
            _interstitial = null;
        }
    }

    void OnDestroy()
    {
        SafeDestroyBanner();
        SafeDestroyInterstitial();
    }

    // ===== Logs =====
    private void Log(string msg) { if (verboseLog) Debug.Log("[Ads] " + msg); }
    private void LogError(string msg) { Debug.LogError("[Ads] " + msg); }
}
