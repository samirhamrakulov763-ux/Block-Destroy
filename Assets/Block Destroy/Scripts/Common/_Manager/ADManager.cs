using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class ADManager : Singleton<ADManager>
{
    // Production IDs (твои реальные ID сохранены)
#if UNITY_ANDROID
    private const string BANNER_AD_UNIT_ID = "ca-app-pub-4928575929411783/6435033796";
    private const string INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-4928575929411783/9811606792";
    private const string REWARDED_AD_UNIT_ID = "ca-app-pub-4928575929411783/2847314012";
#elif UNITY_IOS
    private const string BANNER_AD_UNIT_ID = "ca-app-pub-3940256099942544/2934735716";
    private const string INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/4411468910";
    private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/1712485313";
#else
    private const string BANNER_AD_UNIT_ID = "unused";
    private const string INTERSTITIAL_AD_UNIT_ID = "unused";
    private const string REWARDED_AD_UNIT_ID = "unused";
#endif

    // Тестовые ID для редактора
#if UNITY_EDITOR
    private const string EDITOR_BANNER_AD_UNIT_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string EDITOR_INTERSTITIAL_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string EDITOR_REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917";
#endif

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private Action<bool> rewardedAdCallback;
    private bool isInitialized = false;
    private bool isInitializing = false;

    private int gamesPlayedCount = 0;
    private const int GAMES_BETWEEN_ADS = 5;

    private bool isBannerVisible = false;

    void Start()
    {
        InitializeAds();
        LoadGameCount();
    }

    private void LoadGameCount()
    {
        gamesPlayedCount = PlayerPrefs.GetInt("GamesPlayedCount", 0);
    }

    private void SaveGameCount()
    {
        PlayerPrefs.SetInt("GamesPlayedCount", gamesPlayedCount);
        PlayerPrefs.Save();
    }

    public void IncrementGameCount()
    {
        gamesPlayedCount++;
        SaveGameCount();
    }

    public bool ShouldShowInterstitial()
    {
        bool shouldShow = gamesPlayedCount >= GAMES_BETWEEN_ADS;
        return shouldShow;
    }

    private void ResetGameCount()
    {
        gamesPlayedCount = 0;
        SaveGameCount();
    }

    private string GetCurrentBannerId()
    {
#if UNITY_EDITOR
        return EDITOR_BANNER_AD_UNIT_ID;
#elif UNITY_ANDROID
        return BANNER_AD_UNIT_ID;
#elif UNITY_IOS
        return BANNER_AD_UNIT_ID;
#else
        return "unused";
#endif
    }

    private string GetCurrentInterstitialId()
    {
#if UNITY_EDITOR
        return EDITOR_INTERSTITIAL_AD_UNIT_ID;
#elif UNITY_ANDROID
        return INTERSTITIAL_AD_UNIT_ID;
#elif UNITY_IOS
        return INTERSTITIAL_AD_UNIT_ID;
#else
        return "unused";
#endif
    }

    private string GetCurrentRewardedId()
    {
#if UNITY_EDITOR
        return EDITOR_REWARDED_AD_UNIT_ID;
#elif UNITY_ANDROID
        return REWARDED_AD_UNIT_ID;
#elif UNITY_IOS
        return REWARDED_AD_UNIT_ID;
#else
        return "unused";
#endif
    }

    private void InitializeAds()
    {
        if (isInitialized || isInitializing)
        {
            return;
        }

        isInitializing = true;

        // Configure test devices (optional - for testing with real device)
        List<string> testDeviceIds = new List<string>();
        // Add your test device ID here if needed
        // testDeviceIds.Add("YOUR_DEVICE_ID");

        var requestConfiguration = new RequestConfiguration
        {
            TestDeviceIds = testDeviceIds,
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
            TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.Unspecified,
            MaxAdContentRating = MaxAdContentRating.G
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);


        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            if (initStatus == null)
            {
                isInitializing = false;
                return;
            }

            isInitialized = true;
            isInitializing = false;
            
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #region Banner Ads

    public void ShowBanner()
    {
        if (isBannerVisible) 
        {
            return;
        }

        if (!isInitialized && !Application.isEditor)
        {
            return;
        }

        if (bannerView != null)
        {
            bannerView.Show();
            isBannerVisible = true;
            return;
        }

        // Используем Adaptive Banner вместо Smart Banner
        // ПРЕДУПРЕЖДЕНИЕ: GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth может быть устаревшим
        // Если вы видите ошибки, используйте AdSize.Banner или AdSize.SmartBanner
        int adWidth = Screen.width;  // ← ИСПРАВЛЕНО: конвертируем float в int
        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(adWidth);
        bannerView = new BannerView(GetCurrentBannerId(), adaptiveSize, AdPosition.Bottom);

        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
        isBannerVisible = true;
    }

    public void HideBanner()
    {
        if (!isBannerVisible) return;

        if (bannerView != null)
        {
            bannerView.Hide();
            isBannerVisible = false;
        }
    }

    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
            isBannerVisible = false;
        }
    }

    #endregion

    #region Interstitial Ads

    private void LoadInterstitialAd()
    {
        if (!isInitialized && !Application.isEditor)
        {
            return;
        }

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();
        string interstitialId = GetCurrentInterstitialId();
        
        
        InterstitialAd.Load(interstitialId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }

                interstitialAd = ad;
                RegisterInterstitialEvents(ad);
            });
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            interstitialAd = null;
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            interstitialAd = null;
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
        };
    }

    public void ShowInterstitialAd()
    {
        if (!ShouldShowInterstitial())
        {
            return;
        }

        if (!isInitialized && !Application.isEditor)
        {
            ResetGameCount();
            return;
        }

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            ResetGameCount();
        }
        else
        {
            ResetGameCount();
        }
    }

    public bool IsInterstitialReady()
    {
        bool isReady = interstitialAd != null && interstitialAd.CanShowAd();
        return isReady;
    }

    #endregion

    #region Rewarded Ads

    private void LoadRewardedAd()
    {
        if (!isInitialized && !Application.isEditor)
        {
            return;
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();
        string rewardedId = GetCurrentRewardedId();
        
        
        RewardedAd.Load(rewardedId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }

                rewardedAd = ad;
                RegisterRewardedEvents(ad);
            });
    }

    private void RegisterRewardedEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            rewardedAd = null;
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            rewardedAdCallback?.Invoke(false);
            rewardedAdCallback = null;
            rewardedAd = null;
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
        };
    }

    public void ShowRewardedAd(Action<bool> callback)
    {
        rewardedAdCallback = callback;

        if (!isInitialized && !Application.isEditor)
        {
            rewardedAdCallback?.Invoke(false);
            rewardedAdCallback = null;
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                rewardedAdCallback?.Invoke(true);
                rewardedAdCallback = null;
            });
        }
        else
        {
            rewardedAdCallback?.Invoke(false);
            rewardedAdCallback = null;
        }
    }

    public bool IsRewardedAdReady()
    {
        bool isReady = rewardedAd != null && rewardedAd.CanShowAd();
        return isReady;
    }

    #endregion

    private void OnDestroy()
    {
        DestroyBanner();

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }
}