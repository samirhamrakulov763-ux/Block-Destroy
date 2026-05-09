using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.Purchasing.Extension;
#endif

/// <summary>
/// IAP Manager for Block Destroy
/// Handles all in-app purchases with Unity IAP
/// Includes receipt validation and Firebase integration
/// </summary>
public class IAPManager : Singleton<IAPManager>
#if UNITY_PURCHASING
    , IStoreListener
#endif
{
#if UNITY_PURCHASING
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;
    private bool isInitializing = false;

    public bool IsInitialized => storeController != null && storeExtensionProvider != null;
#else
    public bool IsInitialized => false;
#endif

    // Events
    public event Action<string> OnPurchaseSuccessEvent;
    public event Action<string> OnPurchaseFailedEvent;
    public event Action OnInitializedEvent;

    private bool isProcessingPurchase = false;

    void Start()
    {
#if UNITY_PURCHASING
        InitializePurchasing();
#endif
    }

    /// <summary>
    /// Initialize Unity IAP
    /// </summary>
    public void InitializePurchasing()
    {
#if UNITY_PURCHASING
        if (IsInitialized)
        {
            OnInitializedEvent?.Invoke();
            return;
        }

        if (isInitializing)
        {
            return;
        }

        isInitializing = true;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add all products from catalog
        foreach (var product in IAPProductCatalog.Products)
        {
            builder.AddProduct(product.productId, UnityEngine.Purchasing.ProductType.Consumable);
        }

        // Инициализируем IAP
        UnityPurchasing.Initialize(this, builder);
#endif
    }

    #region Unity IAP Callbacks

#if UNITY_PURCHASING
    /// <summary>
    /// Called when Unity IAP is ready
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        isInitializing = false;

        OnInitializedEvent?.Invoke();
    }

    /// <summary>
    /// Called when Unity IAP initialization fails (old version)
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        isInitializing = false;
    }

    /// <summary>
    /// Called when Unity IAP initialization fails (new version)
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        isInitializing = false;
    }
#endif

    #endregion

    /// <summary>
    /// Purchase a product by ID
    /// </summary>
    public void BuyProduct(string productId)
    {
#if UNITY_PURCHASING
        if (!IsInitialized)
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Store is initializing. Please wait...");
            OnPurchaseFailedEvent?.Invoke(productId);
            return;
        }

        if (isProcessingPurchase)
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Please wait, processing previous purchase...");
            return;
        }

        Product product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            isProcessingPurchase = true;
            storeController.InitiatePurchase(product);
        }
        else
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Product not available.");
            OnPurchaseFailedEvent?.Invoke(productId);
        }
#else
        if (PlayManager.Instance?.commonUI != null)
            PlayManager.Instance.commonUI.SetToast("IAP not enabled in this build.");
        OnPurchaseFailedEvent?.Invoke(productId);
#endif
    }

    #region Purchase Processing

#if UNITY_PURCHASING
    /// <summary>
    /// Called when a purchase completes
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        string transactionId = args.purchasedProduct.transactionID;

        bool validPurchase = true;

        // Validate receipt (skip in editor)
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
        validPurchase = ValidateReceipt(args.purchasedProduct.receipt);
#endif

        if (validPurchase)
        {
            // Проверяем, не обрабатывали ли мы уже эту транзакцию
            if (!IsTransactionAlreadyProcessed(transactionId))
            {
                GrantPurchasedItem(productId);
                MarkTransactionAsProcessed(transactionId);
                SendReceiptToFirebase(productId, args.purchasedProduct.receipt);
                OnPurchaseSuccessEvent?.Invoke(productId);

                if (PlayManager.Instance?.commonUI != null)
                    PlayManager.Instance.commonUI.SetToast($"Purchase successful! You got {IAPProductCatalog.GetProduct(productId)?.rewardAmount} gems!");
            }
            else
            {
                if (PlayManager.Instance?.commonUI != null)
                    PlayManager.Instance.commonUI.SetToast("Purchase already processed.");
            }
        }
        else
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Purchase validation failed.");
            OnPurchaseFailedEvent?.Invoke(productId);
        }

        isProcessingPurchase = false;
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        isProcessingPurchase = false;

        string message = "Purchase failed: ";
        switch (failureReason)
        {
            case PurchaseFailureReason.UserCancelled:
                message = "Purchase cancelled.";
                break;
            case PurchaseFailureReason.PaymentDeclined:
                message = "Payment declined.";
                break;
            case PurchaseFailureReason.DuplicateTransaction:
                message = "Duplicate transaction.";
                break;
            default:
                message += failureReason.ToString();
                break;
        }

        if (PlayManager.Instance?.commonUI != null)
            PlayManager.Instance.commonUI.SetToast(message);

        OnPurchaseFailedEvent?.Invoke(product.definition.id);
    }

    /// <summary>
    /// Validate receipt locally
    /// </summary>
    private bool ValidateReceipt(string receipt)
    {
        bool validPurchase = true;

#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        try
        {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(), Application.identifier);

            var result = validator.Validate(receipt);
        }
        catch (IAPSecurityException)
        {
            validPurchase = false;
        }
#endif

        return validPurchase;
    }

    private bool IsTransactionAlreadyProcessed(string transactionId)
    {
        return PlayerPrefs.GetInt("IAP_" + transactionId, 0) == 1;
    }

    private void MarkTransactionAsProcessed(string transactionId)
    {
        PlayerPrefs.SetInt("IAP_" + transactionId, 1);
        PlayerPrefs.Save();
    }
#endif

    #endregion

    /// <summary>
    /// Grant purchased item to player
    /// </summary>
    private void GrantPurchasedItem(string productId)
    {
        var product = IAPProductCatalog.GetProduct(productId);

        if (product == null)
        {
            return;
        }

        // Grant gems
        GameData.Gem += product.rewardAmount;

        if (PlayManager.Instance?.commonUI?._CoinGem != null)
        {
            PlayManager.Instance.commonUI._CoinGem.SetGem();
        }

        // Publish event
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Publish(new GemChangedEvent(GameData.Gem, product.rewardAmount));
        }

        // Show reward animation
        if (PlayManager.Instance?.commonUI?._GetItem != null)
        {
            PlayManager.Instance.commonUI._GetItem.GetGem(product.rewardAmount, Vector3.zero);
        }

        // Save data
        GameData.Save();
    }

    /// <summary>
    /// Send receipt to Firebase for server-side validation
    /// </summary>
    private void SendReceiptToFirebase(string productId, string receipt)
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.ValidateReceipt(productId, receipt, (success) => { });
        }
    }

    /// <summary>
    /// Get localized price string for a product
    /// </summary>
    public string GetLocalizedPrice(string productId)
    {
#if UNITY_PURCHASING
        if (!IsInitialized)
        {
            return "$?.??";
        }

        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            return product.metadata.localizedPriceString;
        }
#endif
        return "$?.??";
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public IAPProduct GetProduct(string productId)
    {
        return IAPProductCatalog.GetProduct(productId);
    }

    /// <summary>
    /// Check if a product is purchased
    /// </summary>
    public bool IsProductPurchased(string productId)
    {
#if UNITY_PURCHASING
        if (!IsInitialized)
        {
            return false;
        }

        Product product = storeController.products.WithID(productId);
        if (product != null)
        {
            return product.hasReceipt;
        }
#endif
        return false;
    }

    #region iOS Restore Purchases

#if UNITY_IOS && UNITY_PURCHASING
    /// <summary>
    /// Restore purchases on iOS
    /// </summary>
    public void RestorePurchases()
    {
        if (!IsInitialized)
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Store is initializing. Please wait...");
            return;
        }

        var appleExtensions = storeExtensionProvider.GetExtension<IAppleExtensions>();
        if (appleExtensions != null)
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Restoring purchases...");

            appleExtensions.RestoreTransactions((result, error) =>
            {
                if (result)
                {
                    if (PlayManager.Instance?.commonUI != null)
                        PlayManager.Instance.commonUI.SetToast("Purchases restored successfully.");
                }
                else
                {
                    if (PlayManager.Instance?.commonUI != null)
                        PlayManager.Instance.commonUI.SetToast("Restore failed: " + error);
                }
            });
        }
        else
        {
            if (PlayManager.Instance?.commonUI != null)
                PlayManager.Instance.commonUI.SetToast("Restore not available.");
        }
    }
#endif

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Retry initialization if failed
    /// </summary>
    public void RetryInitialization()
    {
        if (IsInitialized)
        {
            OnInitializedEvent?.Invoke();
            return;
        }

        InitializePurchasing();
    }

    #endregion
}