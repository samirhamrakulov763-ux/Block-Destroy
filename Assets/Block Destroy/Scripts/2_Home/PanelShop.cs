using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Advertisements;

public class PanelShop : PanelBase
{

    int freeGemValue = 3;
    int freeCoinValue = 19;


    int[] buyGemValue = new int[6] {30, 80, 170, 360, 950, 2000};
    int[] buyCoinValue = new int[3] {150, 400, 1200};
    int[] buyCoinCost = new int[3] {20, 50, 140};

    public CanvasGroup[] freeList;
    public GameObject buttonNoads;

    public TextMeshProUGUI textAds_Cost;

    public TextMeshProUGUI[] textGemValue;
    public TextMeshProUGUI[] textGemCost;

    public TextMeshProUGUI[] textCoinValue;
    public TextMeshProUGUI[] textCoinCost;

    public Transform[] listTransformGem;
    public Transform[] listTransformCoin;

    private void Start()
    {
        // Subscribe to IAP events
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPurchaseSuccessEvent += OnPurchaseSuccess;
            IAPManager.Instance.OnPurchaseFailedEvent += OnPurchaseFailed;
            IAPManager.Instance.OnInitializedEvent += OnIAPInitialized;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from IAP events
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPurchaseSuccessEvent -= OnPurchaseSuccess;
            IAPManager.Instance.OnPurchaseFailedEvent -= OnPurchaseFailed;
            IAPManager.Instance.OnInitializedEvent -= OnIAPInitialized;
        }
    }

    public override void Open()
    {
        SetFreeButton();

        // Hide Remove Ads button if already purchased
        if (GameData.NoAds && buttonNoads != null)
        {
            buttonNoads.gameObject.SetActive(false);
        }

        for (int i = 0; i < textCoinValue.Length; i++)
        {
            textCoinValue[i].text = Utility.ChangeThousandsSeparator(buyCoinValue[i]);
            textCoinCost[i].text = Utility.ChangeThousandsSeparator(buyCoinCost[i]);
        }

        UpdateGemPrices();
        
        base.Open();
    }

    /// <summary>
    /// Update gem prices from IAP Manager
    /// </summary>
    private void UpdateGemPrices()
    {
        for (int i = 0; i < buyGemValue.Length; i++)
        {
            textGemValue[i].text = Utility.ChangeThousandsSeparator(buyGemValue[i]);

            // Update price from IAP Manager
            if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized)
            {
                string productId = GetGemProductId(i);
                string price = IAPManager.Instance.GetLocalizedPrice(productId);
                if (textGemCost[i] != null)
                {
                    textGemCost[i].text = price;
                }
            }
            else
            {
                // Пока IAP не инициализирован, показываем заглушку
                if (textGemCost[i] != null)
                {
                    textGemCost[i].text = "...";
                }
            }
        }
    }

    /// <summary>
    /// Called when IAP is initialized
    /// </summary>
    private void OnIAPInitialized()
    {
        UpdateGemPrices();
    }

    /// <summary>
    /// Get product ID for gem package
    /// </summary>
    private string GetGemProductId(int index)
    {
        switch (index)
        {
            case 0: return IAPProductCatalog.GEMS_SMALL;
            case 1: return IAPProductCatalog.GEMS_MEDIUM;
            case 2: return IAPProductCatalog.GEMS_LARGE;
            case 3: return IAPProductCatalog.GEMS_HUGE;
            case 4: return IAPProductCatalog.GEMS_MEGA;
            case 5: return IAPProductCatalog.GEMS_ULTIMATE;
            default: return IAPProductCatalog.GEMS_SMALL;
        }
    }

    /// <summary>
    /// Click to purchase gems with real money
    /// </summary>
    public void Click_BuyGemIAP(int id)
    {
        if (IAPManager.Instance == null)
        {
            PlayManager.Instance.commonUI.SetToast("Store not available. Please restart the game.");
            return;
        }

        if (!IAPManager.Instance.IsInitialized)
        {
            PlayManager.Instance.commonUI.SetToast("Store is initializing. Please wait...");
            // Пробуем переинициализировать
            IAPManager.Instance.RetryInitialization();
            return;
        }

        string productId = GetGemProductId(id);

        SoundManager.Instance.PlayEffect(SoundList.sound_common_btn_in);
        IAPManager.Instance.BuyProduct(productId);
    }

    /// <summary>
    /// Click to remove ads - DISABLED (Remove Ads feature removed)
    /// </summary>
    public void Click_RemoveAd()
    {
        PlayManager.Instance.commonUI.SetToast("This feature is not available.");
    }

    /// <summary>
    /// Called when purchase succeeds
    /// Награда уже начислена в IAPManager.GrantPurchasedItem()
    /// Здесь только показываем уведомление
    /// </summary>
    private void OnPurchaseSuccess(string productId)
    {
        
        var product = IAPProductCatalog.GetProduct(productId);
        if (product != null)
        {
            PlayManager.Instance.commonUI.SetToast($"You got {product.rewardAmount} gems!");
        }
        else
        {
            PlayManager.Instance.commonUI.SetToast("Purchase successful!");
        }
    }

    /// <summary>
    /// Called when purchase fails
    /// </summary>
    private void OnPurchaseFailed(string productId)
    {
        PlayManager.Instance.commonUI.SetToast("Purchase failed. Please try again.");
    }

    private bool isClick = false;

    public void Click_Free(int type)
    {
        if (ADManager.Instance == null || !ADManager.Instance.IsRewardedAdReady())
        {
            PlayManager.Instance.commonUI.SetToast("Ad not available. Please try again later.");
            return;
        }

        ADManager.Instance.ShowRewardedAd(success =>
        {
            if (success)
            {
                if (type == 0)
                {
                    PlayManager.Instance.commonUI._GetItem.GetGem(freeGemValue, freeList[0].transform.position);
                }
                else
                {
                    PlayManager.Instance.commonUI._GetItem.GetCoin(freeCoinValue, freeList[1].transform.position);
                }
            }

            SetFreeButton();
        });
    }


    void SetFreeButton()
    {
        if (ADManager.Instance == null || !ADManager.Instance.IsRewardedAdReady())
        {
            freeList[0].DOFade(0.5f, 0f);
            freeList[0].blocksRaycasts = false;
            freeList[1].DOFade(0.5f, 0f);
            freeList[1].blocksRaycasts = false;
        }
        else
        {
            freeList[0].DOFade(1f, 0f);
            freeList[0].blocksRaycasts = true;
            freeList[1].DOFade(1f, 0f);
            freeList[1].blocksRaycasts = true;
        }
    }


    /// <summary>
    /// Buy Click Gem (Old method - DEPRECATED - DO NOT USE)
    /// This method was used for testing and should NOT be called from UI
    /// Use Click_BuyGemIAP() instead
    /// </summary>
    [Obsolete("This method is deprecated. Use Click_BuyGemIAP() instead.")]
    private void Click_BuyGem(int id)
    {
        Debug.LogError("Click_BuyGem() is deprecated! Please use Click_BuyGemIAP() instead. Check your UI button bindings!");
        PlayManager.Instance.commonUI.SetToast("Please use the correct purchase button.");

        // DO NOT grant gems here - this bypasses IAP system
        // Old code removed to prevent accidental free gems
    }

    /// <summary>
    /// Click Coin Purchase (using gems)
    /// </summary>
    public void Click_BuyCoin(int id)
    {
        if (GameData.Gem >= buyCoinCost[id])
        {
            CtrHome ctrHome = PlayManager.Instance.currentBase as CtrHome;

            ctrHome._PopupBuy.buttonOK.onClick.AddListener(() => { BuySuccess(id); });
            ctrHome._PopupBuy.SetGem(id, buyCoinValue[id]);
        }
        else
        {
            SoundManager.Instance.PlayEffect(SoundList.sound_common_sfx_error);
            PlayManager.Instance.commonUI.SetToast("Not enough gems.");
        }
    }

    /// <summary>
    /// After successful purchase
    /// </summary>
    public void BuySuccess(int id)
    {
        PlayManager.Instance.commonUI._GetItem.GetCoin(buyCoinValue[id], listTransformCoin[id].position);
        GameData.Gem -= buyCoinCost[id];
        PlayManager.Instance.commonUI._CoinGem.SetGem();
    }
}