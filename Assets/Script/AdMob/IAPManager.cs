using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    [Header("Product IDs (must match Play Console)")]
    [SerializeField] private string removeAdsProductId = "remove_ads";

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        if (storeController == null) InitializeIAP();
    }

    public void InitializeIAP()
    {
        var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
        var builder = ConfigurationBuilder.Instance(module);

        // Non-consumable remove_ads
        builder.AddProduct(removeAdsProductId, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    // ===== Public API =====
    public void PurchaseNoAds()
    {
        if (storeController == null)
        {
            Debug.LogWarning("[IAP] Store not initialized yet, initializing...");
            InitializeIAP();
            return;
        }
        storeController.InitiatePurchase(removeAdsProductId);
    }

    // Android không có Restore button như iOS, nhưng ta giữ API để bạn map UI nếu muốn.
    public void RestorePurchases()
    {
        Debug.Log("[IAP] RestorePurchases called (Android typically auto-restores via receipt on reinstall).");
        // Với Android, Unity IAP sẽ cung cấp receipt nếu item đã sở hữu khi Initialize xong.
        // Bạn có thể kiểm tra ở OnInitialized().
    }

    // ===== IStoreListener =====
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("[IAP] Initialized.");

        // Nếu user đã mua trước đó, Unity IAP sẽ có receipt cho product non-consumable
        var product = storeController.products.WithID(removeAdsProductId);
        if (product != null && product.hasReceipt)
        {
            Debug.Log("[IAP] remove_ads already owned (receipt found).");
            NoAdsService.Instance?.MarkNoAdsPurchased();
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("[IAP] Initialize failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("[IAP] Initialize failed: " + error + " msg=" + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        if (string.Equals(e.purchasedProduct.definition.id, removeAdsProductId, System.StringComparison.Ordinal))
        {
            Debug.Log("[IAP] Purchase success: remove_ads");
            NoAdsService.Instance?.MarkNoAdsPurchased();
            return PurchaseProcessingResult.Complete;
        }

        Debug.LogWarning("[IAP] Unhandled product: " + e.purchasedProduct.definition.id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} reason={failureReason}");
    }

#if UNITY_2021_2_OR_NEWER
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} reason={failureDescription.reason} message={failureDescription.message}");
    }
#endif
}
