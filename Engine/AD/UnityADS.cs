using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using System;

public class UnityADData : MoeModuleData
{
    public string iosGameId;
    public string androidGameId;
    public bool allowTracking;  // 只针对ios有效

    public UnityADData(string iosGameId, string androidGameId, bool allowTracking)
    {
        this.iosGameId = iosGameId;
        this.androidGameId = androidGameId;
        this.allowTracking = allowTracking;
    }
}

public class UnityADS : MoeAD, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    string _androidGameId;
    string _iOsGameId;
    bool _testMode = true;
    bool _enablePerPlacementMode = true;
    public string gameId { get; private set; }

    const string iOSRewardedAdUnit = "Rewarded_iOS";
    const string AndroidRewardedUnit = "Rewarded_Android";

    private Action<bool> OnRewardedAdComplete = null;

    private string rewardedAdUnit
    {
        get
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return iOSRewardedAdUnit;
            }
            else
            {
                return AndroidRewardedUnit;
            }
        }
    }

    public bool IsInitialized
    {
        get; private set;
    }

    protected override void OnInit(MoeModuleData moeModuleData)
    {
        UnityADData uadd = moeModuleData as UnityADData;
#if UNITY_IOS
        if (uadd.allowTracking)
        {
            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", "true");
            Advertisement.SetMetaData(gdprMetaData);

            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("consent", "true");
            Advertisement.SetMetaData(privacyMetaData);
        }
        else
        {
            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", "false");
            Advertisement.SetMetaData(gdprMetaData);

            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("consent", "false");
            Advertisement.SetMetaData(privacyMetaData);
        }
#endif

        InitializeAds(uadd.iosGameId, uadd.androidGameId);
    }

    private void InitializeAds(string iosGameId, string androidGameId)
    {
        _iOsGameId = iosGameId;
        _androidGameId = androidGameId;
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsGameId
            : _androidGameId;

        _testMode = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor;
        Advertisement.Initialize(gameId, _testMode, _enablePerPlacementMode, this);

        StartCoroutine(DelayLoadAd());
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        IsInitialized = true;
        LoadRewardedAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        StartCoroutine(DelayInit());
    }

    IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(1.0f);
        InitializeAds(_iOsGameId, _androidGameId);
    }


    public void LoadRewardedAd()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (!Advertisement.IsReady(rewardedAdUnit))
        {
            Debug.LogFormat("加载广告: {0}", rewardedAdUnit);
            Advertisement.Load(rewardedAdUnit, this);
        }
        else
        {
            // Debug.LogFormat("广告 {0} 已经等待播放!", rewardedAdUnit);
        }
    }

    public bool IsRewardedAdReady()
    {
        if (!IsInitialized)
        {
            return false;
        }
        return Advertisement.IsReady(rewardedAdUnit);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.LogFormat("AdLoaded: {0}", adUnitId);
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
        // StartCoroutine(DelayLoadAd());
    }

    IEnumerator DelayLoadAd()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (IsInitialized)
            {
                LoadRewardedAd();
            }
        }
    }

    protected override void DoShowRewardedAd(Action<bool> callback)
    {
        OnRewardedAdComplete = callback;
        _ShowRewardedAd();
    }

    void _ShowRewardedAd()
    {
        if (IsRewardedAdReady())
        {
            Advertisement.Show(rewardedAdUnit, this);
        }
        else
        {
            Debug.LogErrorFormat("RewardedAdUnit Not Load");
            RewardedAdCallback(false);
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
        RewardedAdCallback(false);
        LoadRewardedAd();
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.LogFormat("广告开始播放!");
        // LoadRewardedAd();
        MoeAnalyst.Inst.TrackEvent("ADStarted");
    }



    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward.
            RewardedAdCallback(true);
            // Load another ad:
            MoeAnalyst.Inst.TrackEvent("ADCompleted");
        }
        else
        {
            RewardedAdCallback(false);
        }

        //Advertisement.Load(adUnitId, this);
        LoadRewardedAd();
    }

    private void RewardedAdCallback(bool isComplete)
    {
        OnRewardedAdComplete?.Invoke(isComplete);
        OnRewardedAdComplete = null;
    }

}