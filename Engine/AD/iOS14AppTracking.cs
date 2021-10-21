using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS

using Unity.Advertisement.IosSupport;

#endif

public class iOS14AppTracking
{
    public static void ShowAppTracking()
    {
#if UNITY_IOS


        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
        ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            Debug.LogFormat("显示隐私追踪相关的界面");
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }

        Debug.LogFormat("隐私追踪结果: {0}", ATTrackingStatusBinding.GetAuthorizationTrackingStatus());

#endif
    }

    public static bool IsAllowAppTracking()
    {
#if UNITY_IOS
        Debug.LogFormat("获取隐私追踪状态: {0}", ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
        return ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
#else
        return false;
#endif
    }
}
