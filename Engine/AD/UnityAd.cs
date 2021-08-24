//using System.Collections;
//using UnityEngine;
//using UnityEngine.Advertisements;

//public class UnityAd : MonoBehaviour, IUnityAdsListener
//{
//    const string mySurfacingId = "video";
//    const string bannerSurfacingId = "bannerPlacement";
//    private bool testMode = false;
//    protected override void InitAD()
//    {
//        string gameId = string.Empty;

//        Debug.LogFormat("初始化UnityADS, Debug： {0}  {1}", testMode, gameId);
//        Advertisement.AddListener(this);
//        if (Application.platform == RuntimePlatform.WindowsEditor)
//        {
//            Advertisement.Initialize(gameId, true);
//        }
//        else
//        {
//            if (!string.IsNullOrEmpty(DataConfig.Inst.UnityGameIdAndroid) && !string.IsNullOrEmpty(DataConfig.Inst.UnityGameIdiOS))
//            {
//                 gameId = Application.platform == RuntimePlatform.Android ? DataConfig.Inst.UnityGameIdAndroid : DataConfig.Inst.UnityGameIdiOS;
//                Advertisement.Initialize(gameId, testMode);
//            }
//        }
//    }

  
//    protected override void CloseBanner()
//    {
//        StopCoroutine(ShowBannerWhenInitialized());
//        Advertisement.Banner.Hide(true);
//    }

//    IEnumerator ShowBannerWhenInitialized()
//    {
//        Debug.LogFormat(">>>>>>>>>>>>>>> 准备显示Banner广告： {0}", Advertisement.isInitialized);
//        while (!Advertisement.isInitialized)
//        {
//            yield return new WaitForSeconds(0.5f);
//        }

//        if (base.adSwitch)
//        {
//            Debug.LogFormat(">>>>>>>>>>>>>>> 显示Banner广告");
//            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
//            Advertisement.Banner.Show(bannerSurfacingId);
//        }
//    }

//    protected override void OnRequestInterstitial()
//    {
//    }

//    protected override bool OnShowInterstitialAd()
//    {
//        if (Advertisement.IsReady())
//        {
//            MoeAnalyst.Inst.TrackEvent("ShowAD");
//            Advertisement.Show(mySurfacingId);
//            return true;
//        }
//        else
//        {
//            Debug.LogError("广告 Not Ready!");
//            return false;
//        }
//    }

//    // Implement IUnityAdsListener interface methods:
//    public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult)
//    {
//        if(surfacingId != mySurfacingId)
//        {
//            return;
//        }

//        MoeAnalyst.Inst.TrackEvent(string.Format("AD_{0}_End_{1}", surfacingId, showResult.ToString()));

//        base.OnInterstitialADEnd();
//        Debug.LogFormat(">>>>>>>>>>>>>> OnUnityAdsDidFinish: {0}  {1}", surfacingId, showResult);
//        // Define conditional logic for each ad completion status:
//        if (showResult == ShowResult.Finished)
//        {
//            // Reward the user for watching the ad to completion.
//        }
//        else if (showResult == ShowResult.Skipped)
//        {
//            // Do not reward the user for skipping the ad.
//        }
//        else if (showResult == ShowResult.Failed)
//        {
//            Debug.LogWarning("The ad did not finish due to an error.");
//        }
//    }

//    public void OnUnityAdsReady(string surfacingId)
//    {
//        Debug.LogFormat(">>>>>>>>>>>>>> OnUnityAdsReady: {0}", surfacingId);
//        // If the ready Ad Unit or legacy Placement is rewarded, show the ad:
//        if (surfacingId == mySurfacingId)
//        {
//            // Optional actions to take when theAd Unit or legacy Placement becomes ready (for example, enable the rewarded ads button)
//        }
//    }

//    public void OnUnityAdsDidError(string message)
//    {
//        Debug.LogFormat(">>>>>>>>>>>>>> OnUnityAdsDidError: {0}", message);
//        // Log the error.
//    }

//    public void OnUnityAdsDidStart(string surfacingId)
//    {
//        MoeAnalyst.Inst.TrackEvent(string.Format("ADStart_{0}", surfacingId));
//        Debug.LogFormat(">>>>>>>>>>>>>> OnUnityAdsDidStart: {0}", surfacingId);
//        // Optional actions to take when the end-users triggers an ad.
//    }

//    // When the object that subscribes to ad events is destroyed, remove the listener:
//    public void OnDestroy()
//    {
//        Advertisement.RemoveListener(this);
//    }

//}