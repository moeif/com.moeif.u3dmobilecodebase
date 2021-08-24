using System.Collections;
using UnityEngine;
using System;

#if Admob
using GoogleMobileAds.Api;

public class GoogleAdmob : MoeAD
{
    private bool inited = false;
    private BannerView bannerView;
    private InterstitialAd interstitial;

    private bool interestitialConsumed = false;

    protected override void InitAD()
    {
        MobileAds.Initialize(initStatus =>
        {
            inited = true;
            Debug.LogFormat("Google Admob Init: {0}", initStatus);
        });
    }

    protected override void OnRequestBanner()
    {
        // 正式广告单元
//#if UNITY_EDITOR
//        string adUnitId = "unused";
//#elif UNITY_ANDROID
//        string adUnitId = "ca-app-pub-3110170387295963/5399278442";
//#elif UNITY_IPHONE
//        string adUnitId = "ca-app-pub-3110170387295963/4832980852";
//#else
//        string adUnitId = "unexpected_platform";
//#endif

        // 测试广告单元
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        //// Called when an ad request has successfully loaded.
        //this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        //// Called when an ad request failed to load.
        //this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        //// Called when an ad is clicked.
        //this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        //// Called when the user returned from the app after an ad click.
        //this.bannerView.OnAdClosed += this.HandleOnAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }


    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log(">>>>>>>>>>>>>>>> Google Admob HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log(">>>>>>>>>>>>>>>> Google Admob HandleFailedToReceiveAd event received with message: "
                            + args.LoadAdError);
        RequestInterstitial();
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log(">>>>>>>>>>>>>>>> Google Admob HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log(">>>>>>>>>>>>>>>> Google Admob HandleAdClosed event received");
        RequestInterstitial();
    }

    protected override void OnRequestInterstitial()
    {
        if (IsInterstitialADLoaded())
        {
            Debug.LogFormat("广告已准备好，等待播放");
            return;
        }

        // 正式广告ID
        //#if UNITY_EDITOR
        //        string adUnitId = "unused";
        //#elif UNITY_ANDROID
        //        string adUnitId = "ca-app-pub-3110170387295963/2334131403";
        //#elif UNITY_IPHONE
        //        string adUnitId = "ca-app-pub-3110170387295963/5543427294";
        //#else
        //        string adUnitId = "unexpected_platform";
        //#endif

        // 测试广告ID
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif

        if(this.interstitial != null)
        {
            this.interstitial.Destroy();
        }

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += this.HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        this.interstitial.OnAdOpening += this.HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        this.interstitial.OnAdClosed += this.HandleOnAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    protected override bool OnShowInterstitialAd()
    {
        if (interstitial != null && interstitial.IsLoaded())
        {
            interstitial.Show();
            interestitialConsumed = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override bool OnIsInterstitialADLoaded()
    {
        return this.interstitial != null && this.interstitial.IsLoaded();
    }

}
#endif