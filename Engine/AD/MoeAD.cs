#define UnityAd
using System.Collections;
using UnityEngine;
using System;

public class MoeAD : MonoBehaviour
{
    private static MoeAD _inst = null;
    public static MoeAD Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject obj = new GameObject("MoeAD");
                DontDestroyOnLoad(obj);
#if Admob
                _inst = obj.AddComponent<GoogleAdmob>();
#elif UnityAd
                UnityADS uAds = obj.AddComponent<UnityADS>();
                _inst = uAds;
#else
                _inst = obj.AddComponent<MoeAD>();
#endif
            }
            return _inst;
        }
    }

    public void Init(MoeModuleData mmd)
    {
        OnInit(mmd);
    }

    protected virtual void OnInit(MoeModuleData moeModuleData)
    {

    }

    private void Start()
    {
        //MoeEventManager.Inst.RegisterEvent(EventID.OnSubscribeSuccess, this.OnEvent);
    }

    private void OnEvent(int eventId, MoeEventParam eParam)
    {
        //if(eventId == EventID.OnSubscribeSuccess)
        //{
        //    SwitchAD(!ProAgent.Inst.IsProUser);
        //}
    }

    public void Init()
    {
        // do nothing
    }

    /// <summary>
    /// 默认广告是开的
    /// </summary>
    protected bool adSwitch = true;

    public void SwitchAD(bool adSwitch)
    {
        Debug.LogFormat("设置广告开关: {0}", adSwitch);
        this.adSwitch = adSwitch;
        if (!adSwitch)
        {
            Debug.LogFormat("关闭横幅广告");
        }
    }

    public void ShowRewardedAd(Action<bool> callback)
    {
        DoShowRewardedAd(callback);
    }

    protected virtual void DoShowRewardedAd(Action<bool> callback)
    {

    }



}