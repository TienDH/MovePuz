using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    private BannerView bannerView;
    private InterstitialAd interstitialAd;

    [Header("AdMob Ad Unit IDs (Test IDs)")]
    private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test Banner
    private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test Interstitial

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Initialized");
            RequestBannerAd();
            RequestInterstitialAd();
        });
    }

    #region Banner Ad
    public void RequestBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
        Debug.Log("Banner Ad Requested");
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
            Debug.Log("Banner Shown");
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            Debug.Log("Banner Hidden");
        }
    }
    #endregion

    #region Interstitial Ad
    public void RequestInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Failed to load interstitial ad: " + error.GetMessage());
                return;
            }

            interstitialAd = ad;
            Debug.Log("Interstitial Ad Loaded");

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial Ad Closed");
                RequestInterstitialAd(); // Tải lại sau khi xem
            };
        });
    }

    public bool IsInterstitialAdReady()
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }

    public void ShowInterstitialAd()
    {
        if (IsInterstitialAdReady())
        {
            interstitialAd.Show();
            Debug.Log("Interstitial Ad Shown");
        }
        else
        {
            Debug.Log("Interstitial Ad not ready yet, requesting...");
            RequestInterstitialAd();
        }
    }
    #endregion

    void OnDestroy()
    {
        if (bannerView != null)
            bannerView.Destroy();

        if (interstitialAd != null)
            interstitialAd.Destroy();
    }
}
