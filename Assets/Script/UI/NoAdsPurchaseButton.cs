using UnityEngine;
using UnityEngine.UI;

public class NoAdsPurchaseButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject thankYouPanel; // đặt inactive mặc định
    [SerializeField] private bool closeOnAndroidBack = true; // Back key tắt panel
    private Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
        if (_btn)
        {
            _btn.onClick.RemoveAllListeners();
            _btn.onClick.AddListener(HandleClick);
        }

        // đảm bảo panel tắt lúc khởi động (nếu bạn muốn)
        if (thankYouPanel && !Application.isPlaying)
            thankYouPanel.SetActive(false);
    }

    void Update()
    {
        // Android nút Back để đóng panel
        if (closeOnAndroidBack && thankYouPanel && thankYouPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseThankYou();
        }
    }

    void HandleClick()
    {
        bool isNoAds = NoAdsService.Instance && NoAdsService.Instance.IsNoAds;

        if (isNoAds)
        {
            // ĐÃ MUA → Toggle panel cảm ơn
            if (!thankYouPanel) { Debug.Log("[NoAdsBtn] ThankYouPanel chưa gán."); return; }
            thankYouPanel.SetActive(!thankYouPanel.activeSelf);
            return;
        }

        // CHƯA MUA → Gọi mua
        var iap = FindAnyObjectByType<IAPManager>(); // từ Bootstrap (DontDestroyOnLoad)
        if (iap != null)
        {
            if (_btn) { _btn.interactable = false; Invoke(nameof(Reenable), 2f); }
            iap.PurchaseNoAds();
        }
        else
        {
            Debug.LogWarning("[NoAdsBtn] Không tìm thấy IAPManager (Bootstrap?).");
        }
    }

    public void CloseThankYou()  // có thể gán vào nút "Close" trên panel
    {
        if (thankYouPanel) thankYouPanel.SetActive(false);
    }

    void Reenable() { if (_btn) _btn.interactable = true; }
}
