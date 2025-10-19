using UnityEngine;

public class ClassicBannerBinder : MonoBehaviour
{
    void OnEnable()
    {
        // Yêu cầu tải banner và hiện nó khi vào scene Classic
        var ads = FindAnyObjectByType<AdsManager>();
        ads?.RequestBannerAd();
        ads?.ShowBanner();
    }

    void OnDisable()
    {
        // Ẩn banner khi rời scene (để Menu/scene khác tự quyết)
        var ads = FindAnyObjectByType<AdsManager>();
        ads?.HideBanner();
    }
}
