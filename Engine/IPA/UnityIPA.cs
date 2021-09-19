#if UNITY_IPA
#define SUBSCRIPTION_MANAGER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class UnityIPA : IStoreListener
{
    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    private IAppleExtensions m_AppleExtensions;

    public bool IsInitializeFaild { get; private set; }

    private bool enterPurchasing = false;
    public UnityIPA()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }


    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        IsInitializeFaild = false;

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add a product to sell / restore by way of its identifier, associating the general identifier
        // with its store-specific identifiers.
        //builder.AddProduct(ProAgent.kProductIdMonthly, ProductType.Subscription);
        //builder.AddProduct(ProAgent.kProductIdQuarterly, ProductType.Subscription);
        //builder.AddProduct(ProAgent.kProductIdYearly, ProductType.Subscription);
        builder.AddProduct(ProAgent.kProductIdPermanent, ProductType.NonConsumable);
        builder.AddProduct(ProAgent.kProductIdMontly, ProductType.Subscription);

        // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
        // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
        UnityPurchasing.Initialize(this, builder);
    }


    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public string GetProductPriceFromStore(string productId)
    {
        if(m_StoreController != null && m_StoreController.products != null)
        {
            return m_StoreController.products.WithID(productId).metadata.localizedPriceString;
        }
        return string.Empty;
    }

    public Product GetProductInfo(string productId)
    {
        if(m_StoreController != null && m_StoreController.products != null)
        {
            return m_StoreController.products.WithID(productId);
        }
        return null;
    }

    public void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            //GameDriver.Inst.analyst.OnClickPayment(productId);
            MoeAnalyst.Inst.TrackEvent("OnBuyVIP");

            enterPurchasing = true;
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format(">>>>>>>> Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                MoeEventManager.Inst.SendEvent(EventID.OnPaymentProcessEnd);
                // ... report the product look-up failure situation  
                Debug.Log(">>>>>>>> BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            MoeEventManager.Inst.SendEvent(EventID.OnPaymentProcessEnd);
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log(">>>>>>>> BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log(">>>>>>>> RestorePurchases FAIL. Not initialized.");
            //MoeEventManager.Inst.SendEvent(EventID.OnRestoreEnd);

            //string msg = GameDataUtil.GetLanguage(10073);
            //UIMessageBoxPanelData uiData = new UIMessageBoxPanelData(msg, null, null, "OK");

            //UIManager.OpenPanel(UIEnum.UIMessageBoxPanel, 1, uiData);

            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log(">>>>>>>> RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log(">>>>>>>> RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                MoeEventManager.Inst.SendEvent(EventID.OnRestoreEnd);
            });
            //GameDriver.Inst.analyst.OnClickRestore();
            MoeAnalyst.Inst.TrackEvent("RestoreVIP");
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log(">>>>>>>> RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            MoeEventManager.Inst.SendEvent(EventID.OnRestoreEnd);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log(">>>>>>>> OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;

        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();


#if SUBSCRIPTION_MANAGER
        Dictionary<string, string> introductory_info_dict = m_AppleExtensions.GetIntroductoryPriceDictionary();
#endif

        bool onProPeroid = false;

        Debug.Log(">>>>>>>>>>>>>> Available items:");
        foreach (var item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                Debug.Log(string.Join(" - ",
                    new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt
                    }));

#if SUBSCRIPTION_MANAGER
                // this is the usage of SubscriptionManager class
                if (item.receipt != null)
                {
                    if (item.definition.type == ProductType.Subscription)
                    {
                        if (checkIfProductIsAvailableForSubscriptionManager(item.receipt))
                        {
                            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
                            SubscriptionManager p = new SubscriptionManager(item, intro_json);
                            SubscriptionInfo info = p.getSubscriptionInfo();
                            Debug.Log("product id is: " + info.getProductId());
                            Debug.Log("purchase date is: " + info.getPurchaseDate());
                            Debug.Log("subscription next billing date is: " + info.getExpireDate());
                            Debug.Log("is subscribed? " + info.isSubscribed().ToString());
                            Debug.Log("is expired? " + info.isExpired().ToString());
                            Debug.Log("is cancelled? " + info.isCancelled());
                            Debug.Log("product is in free trial peroid? " + info.isFreeTrial());
                            Debug.Log("product is auto renewing? " + info.isAutoRenewing());
                            Debug.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                            Debug.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                            Debug.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
                            Debug.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                            Debug.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());

                            if (info.isFreeTrial() == Result.True || !(info.isExpired() == Result.True))
                            {
                                Debug.LogFormat("这个商品触发Pro版本: {0}", info.getProductId());
                                onProPeroid = true;
                            }
                        }
                        else
                        {
                            Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
                        }
                    }
                    else
                    {
                        if(item.definition.type == ProductType.NonConsumable) {
                            onProPeroid = true;
                            Debug.LogFormat("永久购买触发Pro版本: {0}", item.metadata.localizedTitle);
			            }
                        Debug.Log("the product is not a subscription product");
                    }
                }
                else
                {
                    Debug.Log("the product should have a valid receipt");
                }
#endif
            }
	    }

        if (onProPeroid) {
            Debug.LogFormat("初始化时，发现用户处于Pro版本期限");
            ProAgent.Inst.OnProPeroid(true);
	    }
        else
        {
            Debug.LogFormat("初始化时，发现当前用户是非付费用户");
            ProAgent.Inst.OnProPeroid(false);
        }
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log(">>>>>>>> OnInitializeFailed InitializationFailureReason:" + error);
        IsInitializeFaild = true;
        //GameDriver.Inst.analyst.OnInitFaild();
        MoeAnalyst.Inst.TrackEvent("IPAInitFailed");
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // A consumable product has been purchased by this user.
        //if (String.Equals(args.purchasedProduct.definition.id, ProAgent.kProductIdMonthly, StringComparison.Ordinal))
        //{
        //    Debug.Log(string.Format(">>>>>>>> ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
        //    // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
        //    // todo: bought monthly
        //    UnlockProduct(ProAgent.kProductIdMonthly);
        //}
        //// Or ... a non-consumable product has been purchased by this user.
        //else if (String.Equals(args.purchasedProduct.definition.id, ProAgent.kProductIdQuarterly, StringComparison.Ordinal))
        //{
        //    Debug.Log(string.Format(">>>>>>>> ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
        //    // TODO: The non-consumable item has been successfully purchased, grant this item to the player.
        //    UnlockProduct(ProAgent.kProductIdQuarterly);
        //}
        //// Or ... a subscription product has been purchased by this user.
        //else if (String.Equals(args.purchasedProduct.definition.id, ProAgent.kProductIdYearly, StringComparison.Ordinal))
        //{
        //    Debug.Log(string.Format(">>>>>>>> ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
        //    // TODO: The subscription item has been successfully purchased, grant this to the player.
        //    UnlockProduct(ProAgent.kProductIdYearly);
        //}
        if(String.Equals(args.purchasedProduct.definition.id, ProAgent.kProductIdPermanent, StringComparison.Ordinal))
        {
            UnlockProduct(ProAgent.kProductIdPermanent);
        }
        // Or ... an unknown product has been purchased by this user. Fill in additional products here....
        else
        {
            Debug.Log(string.Format(">>>>>>>> ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        MoeEventManager.Inst.SendEvent(EventID.OnPaymentProcessEnd);

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }

    private void UnlockProduct(string productId)
    {
        // 购买完成，解锁商品
        Debug.LogFormat(">>>>>>>> 解锁商品: {0}", productId);
        ProAgent.Inst.OnUnlockProduct(productId);

        if (enterPurchasing)
        {
            enterPurchasing = false;
            int count = 0;

            switch (productId) {
                //case ProAgent.kProductIdMonthly: count = 1; break;
                //case ProAgent.kProductIdQuarterly: count = 3; break;
                //case ProAgent.kProductIdYearly: count = 12; break;
                case ProAgent.kProductIdPermanent: count = -1; break;
	        }

            //GameDriver.Inst.analyst.OnPaymentSuccessed(productId);
            //ProEffectController.Inst.PlayFire(count);
            MoeAnalyst.Inst.TrackEvent("OnBecomeVIP");
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format(">>>>>>>> OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        MoeEventManager.Inst.SendEvent(EventID.OnPaymentProcessEnd);
        //GameDriver.Inst.analyst.OnPaymentFaild(failureReason.ToString());
        MoeAnalyst.Inst.TrackEvent("PaymentFaild_" + failureReason.ToString());
    }





#if SUBSCRIPTION_MANAGER
    private bool checkIfProductIsAvailableForSubscriptionManager(string receipt)
    {
        var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
        if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload"))
        {
            Debug.Log("The product receipt does not contain enough information");
            return false;
        }
        var store = (string)receipt_wrapper["Store"];
        var payload = (string)receipt_wrapper["Payload"];

        if (payload != null)
        {
            switch (store)
            {
                case GooglePlay.Name:
                    {
                        var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                        if (!payload_wrapper.ContainsKey("json"))
                        {
                            Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                            return false;
                        }
                        var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                        if (original_json_payload_wrapper == null || !original_json_payload_wrapper.ContainsKey("developerPayload"))
                        {
                            Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                            return false;
                        }
                        var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                        var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                        if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial"))
                        {
                            Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                            return false;
                        }
                        return true;
                    }
                case AppleAppStore.Name:
                case AmazonApps.Name:
                case MacAppStore.Name:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
        return false;
    }
#endif
}
#endif
