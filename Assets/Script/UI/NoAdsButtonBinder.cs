using UnityEngine;
using UnityEngine.UI;

public class NoAdsButtonBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject noAdsButtonRoot;   // chính nút hoặc container của nút
    [SerializeField] private GameObject noAdsBadge;        // (tuỳ chọn) icon/ô “No Ads”
    [SerializeField] private GameObject thankYouPanel;     // Panel cảm ơn (ẩn mặc định)
    [SerializeField] private Button button;                // (tuỳ chọn) tự tìm nếu để trống

    void Awake()
    {
        if (!button) button = GetComponentInChildren<Button>(true);
        if (!noAdsButtonRoot) noAdsButtonRoot = (button ? button.gameObject : gameObject);
    }

    void OnEnable()
    {
        UpdateUI();
        if (NoAdsService.Instance)
            NoAdsService.Instance.OnNoAdsChanged += HandleNoAdsChanged;
    }

    void OnDisable()
    {
        if (NoAdsService.Instance)
            NoAdsService.Instance.OnNoAdsChanged -= HandleNoAdsChanged;
    }

    void Start() => UpdateUI();

    void HandleNoAdsChanged(bool _) => UpdateUI();

    void UpdateUI()
    {
        // KHÔNG ẩn nút sau khi mua nữa
        if (noAdsButtonRoot) noAdsButtonRoot.SetActive(true);

        // Bật badge khi đã mua (nếu có)
        bool isNoAds = NoAdsService.Instance && NoAdsService.Instance.IsNoAds;
        if (noAdsBadge) noAdsBadge.SetActive(isNoAds);
    }

    // Gắn hàm này vào OnClick của nút No Ads
    public void OnClick_NoAds()
    {
        bool isNoAds = NoAdsService.Instance && NoAdsService.Instance.IsNoAds;

        if (isNoAds)
        {
            // Đã mua → chỉ mở panel cảm ơn
            if (thankYouPanel) thankYouPanel.SetActive(true);
            else Debug.Log("[NoAdsButton] ThankYouPanel chưa gán.");
        }
        else
        {
            // Chưa mua → gọi mua
            var iap = FindAnyObjectByType<IAPManager>();
            if (iap != null)
            {
                // (tuỳ chọn) khoá nút trong 2s để tránh spam
                if (button) { button.interactable = false; Invoke(nameof(Reenable), 2f); }
                iap.PurchaseNoAds();
            }
            else
            {
                Debug.LogWarning("[NoAdsButton] Không tìm thấy IAPManager.");
            }
        }
    }

    void Reenable()
    {
        if (button) button.interactable = true;
    }
}
