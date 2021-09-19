using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class UILayerConfig
{
    public UILayer layer;
    public Transform parent;
    public int startSortingLayer;
}

public enum UILayer
{
    Panel,
    Constant,
    Popup,
    Highest,
}

public class UIConfig
{
    public int uiKey;
    public string uiResName;
    public UILayer uiLayer = UILayer.Panel;

    public UIConfig(int uiKey, string uiResName, UILayer uiLayer = UILayer.Panel)
    {
        this.uiKey = uiKey;
        this.uiResName = uiResName;
        this.uiLayer = uiLayer;
    }
}

public static partial class UIConfigs
{
    private static Dictionary<int, UIConfig> dataDict = new Dictionary<int, UIConfig>();

    private static void AddUIConfig(UIConfig uiConfig)
    {
        dataDict.Add(uiConfig.uiKey, uiConfig);
    }

    public static UIConfig GetUIConfig(int uiKey)
    {
        UIConfig result = null;
        dataDict.TryGetValue(uiKey, out result);
        return result;
    }
}


public class UIManager : MonoBehaviour
{
    public static UIManager Inst { get; private set; }
    public UILayerConfig[] layerConfigs;

    private Dictionary<UILayer, UILayerConfig> layerConfigDict = new Dictionary<UILayer, UILayerConfig>();
    private List<UIQueueItem> uiQueue = new List<UIQueueItem>();

    private void Awake()
    {
        Inst = this;

        if (layerConfigs != null)
        {
            for (int i = 0; i < layerConfigs.Length; ++i)
            {
                if (!layerConfigDict.ContainsKey(layerConfigs[i].layer))
                {
                    layerConfigDict.Add(layerConfigs[i].layer, layerConfigs[i]);
                }
            }
        }
    }

    class UIQueueItem
    {
        public UIConfig uiConfig;
        public UIPanel uiPanel;
        public int sortingOrder;
        public object uiData;
        public Canvas uiCanvas;
        public int signature;

        public UIQueueItem(UIConfig uiConfig)
        {
            this.uiConfig = uiConfig;
        }

        public void ResetOrder(int sortingOrder)
        {
            this.sortingOrder = sortingOrder;
            if (uiCanvas != null)
            {
                uiCanvas.sortingOrder = this.sortingOrder;
            }
        }

        public bool DataDirty(object uiData, int signature)
        {
            bool dataDirty = true;
            if (this.uiData != null && uiData != null && this.uiData.Equals(uiData))
            {
                dataDirty = false;
            }
            else
            {
                if (this.uiData == null && uiData == null)
                {
                    dataDirty = false;
                }
                else
                {
                    dataDirty = true;
                }
            }

            return dataDirty;
        }

        public void Open(object uiData, int signature = 1)
        {
            this.signature = signature;
            bool dataDirty = true;

            if (this.uiData != null && uiData != null && this.uiData.Equals(uiData))
            {
                dataDirty = false;
            }
            else
            {
                if (this.uiData == null && uiData == null)
                {
                    dataDirty = false;
                }
                else
                {
                    dataDirty = true;
                    this.uiData = uiData;
                }
            }

            if (uiPanel != null && dataDirty)
            {
                this.uiPanel.OpenUI(this.uiData);
            }

            if (this.uiPanel != null)
            {
                SetUIPanel();
            }
        }

        public void ActiveUI()
        {
            if (uiPanel != null)
            {
                this.uiPanel.ActiveUI();
            }
        }

        public void OnPanelLoaded(UIPanel uiPanel)
        {
            this.uiPanel = uiPanel;
            this.uiCanvas = this.uiPanel.GetComponent<Canvas>();
            //this.uiPanel.OpenUI(this.uiData);
            Debug.LogFormat("==== 1 On Panel Loaded: {0}", uiPanel.name);
            this.uiPanel.SetData(this.uiData);
            this.SetUIPanel();
        }

        private void SetUIPanel()
        {
            this.uiCanvas.sortingOrder = this.sortingOrder;
            this.uiPanel.uiKey = uiConfig.uiKey;
            this.uiPanel.PanelLayer = uiConfig.uiLayer;
            this.uiPanel.Signature = this.signature;
        }
    }

    private UIQueueItem GetUIQueueItem(int uiKey, int signature)
    {
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiKey == uiKey && uiQueue[i].signature == signature)
            {
                return uiQueue[i];
            }
        }
        return null;
    }


    public static void OpenPanel(int uiKey, int signature = 1, object data = null)
    {
        Inst.OpenUI(uiKey, signature, data);
    }

    public void OpenUI(int uiKey, int signature = 1, object data = null)
    {
        UIQueueItem uiItem = GetUIQueueItem(uiKey, signature);
        UIConfig uiConfig = UIConfigs.GetUIConfig(uiKey);
        if (uiItem == null)
        {
            if (uiConfig != null)
            {
                UIQueueItem uqi = new UIQueueItem(uiConfig);
                uiQueue.Add(uqi);
                CorrectingSortingLayer(uiConfig.uiLayer);
                uqi.Open(data, signature);
                LoadUI(uqi);
            }
            else
            {
                Debug.LogErrorFormat("未找到{0}的配置", uiKey);
            }
        }
        else
        {
            UIQueueItem topItem = GetTopItem(uiConfig.uiLayer, signature);
            if (topItem != uiItem || uiItem.DataDirty(data, signature))
            {
                // 改变sortinglayer，调到最上层
                uiQueue.Remove(uiItem);
                uiQueue.Add(uiItem);
                CorrectingSortingLayer(uiItem.uiConfig.uiLayer);
                uiItem.Open(data, signature);
                RefreshUIQueue();
                uiItem.ActiveUI();
            }
        }
    }

    public static void ClosePanel(int uiKey, int signature = 1)
    {
        Inst.CloseUI(uiKey, signature);
    }

    public void CloseUI(int uiKey, int signature)
    {
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiKey == uiKey && uiQueue[i].signature == signature)
            {
                UIQueueItem uqi = uiQueue[i];
                uiQueue.RemoveAt(i);
                GameObject.Destroy(uqi.uiPanel.gameObject);
                break;
            }
        }

        RefreshUIQueue();
    }

    public void CloseAll()
    {
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            GameObject.Destroy(uiQueue[i].uiPanel.gameObject);
        }

        uiQueue.Clear();
    }

    public void CloseUI(int uiKey)
    {
        for (int i = uiQueue.Count - 1; i >= 0; --i)
        {
            if (uiQueue[i].uiConfig.uiKey == uiKey)
            {
                UIQueueItem uqi = uiQueue[i];
                uiQueue.RemoveAt(i);
                GameObject.Destroy(uqi.uiPanel.gameObject);
            }
        }

        RefreshUIQueue();
    }

    public void CloseUI(UIPanel uiPanel)
    {
        int uiIndex = -1;
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiPanel == uiPanel)
            {
                uiIndex = i;
                break;
            }
        }

        if (uiIndex >= 0)
        {
            UIQueueItem uqi = uiQueue[uiIndex];
            uiQueue.RemoveAt(uiIndex);
            GameObject.Destroy(uqi.uiPanel.gameObject);
            RefreshUIQueue();
        }
    }

    private void LoadUI(UIQueueItem uqi)
    {
        MoeResManager.Inst.LoadAssetAsync<GameObject>(uqi.uiConfig.uiResName, (GameObject obj) =>
        {
            GameObject uiObj = Instantiate(obj, transform);
            UIQueueItem currUqi = GetUIQueueItem(uqi.uiConfig.uiKey, uqi.signature);
            if (currUqi != null)
            {
                currUqi.OnPanelLoaded(uiObj.GetComponent<UIPanel>());
                RefreshUIQueue();
                uiObj.transform.SetParent(layerConfigDict[uqi.uiConfig.uiLayer].parent);
            }
            else
            {
                GameObject.Destroy(uiObj);
            }
        });
    }

    private void RefreshUIQueue()
    {
        //UIQueueItem uqi = null;
        //for(int i = uiQueue.Count - 1; i >= 0; --i)
        //{
        //    UIQueueItem checkUQI = uiQueue[i];
        //    if(checkUQI.uiConfig.uiLayer == UILayer.Panel)
        //    {
        //        if(uqi == null)
        //        {
        //            uqi = checkUQI;
        //            if (checkUQI.uiCanvas != null)
        //            {
        //                checkUQI.uiCanvas.enabled = true;
        //            }
        //        }
        //        else
        //        {
        //            if (checkUQI.uiCanvas != null)
        //            {
        //                checkUQI.uiCanvas.enabled = false;
        //            }
        //        }
        //    }
        //}
    }

    private void CorrectingSortingLayer(UILayer layer)
    {
        int sortingLayer = -1;
        if (layerConfigDict.ContainsKey(layer))
        {
            sortingLayer = layerConfigDict[layer].startSortingLayer;
        }

        for (int i = 0; i < uiQueue.Count; ++i)
        {
            UIQueueItem uqi = uiQueue[i];
            if (uqi.uiConfig.uiLayer == layer)
            {
                uqi.ResetOrder(sortingLayer++);
            }
        }
    }

    public int GetTopint(UILayer layer)
    {
        int topEnum = -1;
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiLayer == layer)
            {
                topEnum = uiQueue[i].uiConfig.uiKey;
            }
        }

        return topEnum;
    }

    public UIPanel GetPanel(int uiKey)
    {
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiKey == uiKey && uiQueue[i].uiPanel != null)
            {
                return uiQueue[i].uiPanel;
            }
        }
        return null;
    }

    private UIQueueItem GetTopItem(UILayer layer, int signature)
    {
        int index = -1;
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiLayer == layer && uiQueue[i].signature == signature)
            {
                index = i;
            }
        }

        if (index >= 0)
        {
            return uiQueue[index];
        }

        return null;
    }

    public UIPanel GetTopPanel(UILayer layer)
    {
        int index = -1;
        for (int i = 0; i < uiQueue.Count; ++i)
        {
            if (uiQueue[i].uiConfig.uiLayer == layer)
            {
                index = i;
            }
        }

        if (index >= 0)
        {
            return uiQueue[index].uiPanel;
        }

        return null;
    }

    //public void PopupMsg(string msg, EnSystemMsgType msgType)
    //{
    //    UIManager.Inst.OpenUI(int.UIPopupPanel, 1, new UIPopupPanelData(msg, msgType)); 
    //}
}

public enum EnSystemMsgType
{
    Success,
    Warning,
    Error,
}
