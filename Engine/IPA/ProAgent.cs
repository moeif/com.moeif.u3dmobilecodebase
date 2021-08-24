using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Purchasing;

public class ProductsInfoParam : MoeEventParam
{
    public List<Product> productList = null;
}

public class ProAgent : MoeSingleton<ProAgent>
{
    //public const string kProductIdMonthly = "wushisub1month";
    //public const string kProductIdQuarterly = "wushisub3month";
    //public const string kProductIdYearly = "wushisub1year";
    public const string kProductIdPermanent = "lotteryfanspermanentvip";
    public const string kProductIdMontly = "lotteryfansmonthlyvip";

    private const string kLastProStateKey = "com.moeif.lotteryfans.lastprostate";

    private UnityIPA unityIPA { get; set; }

    private bool isProUser = false;
    public bool IsProUser
    {
        get
        {
            return isProUser;
        }
        set
        {
            isProUser = value;
            SaveCurrentProState();
        }
    }

    public bool IsFreeUser
    {
        get
        {
            return !IsProUser;
        }
    }

    public bool IsProductInfoAvailable
    {
        get
        {
#if UNITY_IPHONE
            return unityIPA.IsInitialized();
#else
            return true;
#endif
        }
    }

    protected override void InitOnCreate()
    {
        LoadLastProState();
#if UNITY_IPHONE
        unityIPA = new UnityIPA();
#else

#endif
        StartCoroutine(InitProcess());
    }


    public void BuyProduct(string productId)
    {
#if UNITY_IPHONE
        unityIPA.BuyProductID(productId);
#else
#endif

    }

    public void Restore()
    {
#if UNITY_IPHONE
        unityIPA.RestorePurchases();
#else

#endif
    }

    public Product GetProductsInfo(string productId)
    {
#if UNITY_IPHONE
        return unityIPA.GetProductInfo(productId);
#else
        return null;
#endif
    }

    public void OnUnlockProduct(string productId)
    {
        OnProPeroid(true);
    }

    public void OnProPeroid(bool paymentUser)
    {
        IsProUser = paymentUser;
        MoeEventManager.Inst.SendEvent(EventID.OnSubscribeSuccess);
    }

    IEnumerator InitProcess() {
        yield return new WaitForSeconds(5.0f);

#if UNITY_IPHONE
        WaitForSeconds wait = new WaitForSeconds(3.0f);
        while (!unityIPA.IsInitialized()) {
            yield return wait;
            if (unityIPA.IsInitializeFaild) {
                Debug.LogFormat("重新初始化IPA");
                unityIPA.InitializePurchasing();
	        }
	    }
#else

#endif
    }


    private void LoadLastProState()
    {
        if (PlayerPrefs.HasKey(kLastProStateKey))
        {
            int value = PlayerPrefs.GetInt(kLastProStateKey);
            Debug.LogFormat("读取上次保存的Pro状态: {0}", value);
            if(value == 1)
            {
                isProUser = true;
            }
        }
    }

    private void SaveCurrentProState()
    {
        Debug.LogFormat("保存当前Pro状态: {0}", isProUser ? 1 : 0);
        PlayerPrefs.SetInt(kLastProStateKey, isProUser ? 1 : 0);
    }
}