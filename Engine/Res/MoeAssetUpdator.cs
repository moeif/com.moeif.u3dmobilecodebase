using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;

public class MoeAssetUpdator : MoeSingleton<MoeAssetUpdator>
{
    private System.Action onDoneCallback = null;

    public void UpdateAssets(System.Action onDoneCallback)
    {
        this.onDoneCallback = onDoneCallback;

        TryUpdate();
    }

    // 可以从UI重新调用更新（在失败的时候）
    public void TryUpdate()
    {
        StartCoroutine(UpdateProcess());
    }

    IEnumerator UpdateProcess()
    {
        yield return new WaitForSeconds(0.3f);
        yield return Addressables.InitializeAsync();
        IEnumerable<IResourceLocator> locators = Addressables.ResourceLocators;
        List<object> keys = new List<object>();
        Dictionary<object, long> assetSizeDict = new Dictionary<object, long>();

        long totalDownloadedSize = 0;
        long needUpdateSize = 0;

        bool downloadFaild = false;

        //暴力遍历所有的key
        foreach (var locator in locators)
        {
            foreach (var key in locator.Keys)
            {
                var handle = Addressables.GetDownloadSizeAsync(key);
                yield return handle;
                Debug.LogFormat("Key: {0}  downloadSize: {1}", key, handle.Result);

                if (handle.Result > 0)
                {
                    keys.Add(key);
                    assetSizeDict.Add(key, handle.Result);
                    needUpdateSize += handle.Result;
                }
            }
        }

        foreach (object key in keys)
        {
            long itemSize = assetSizeDict[key];
            var handle = Addressables.DownloadDependenciesAsync(key, false);
            while (!handle.IsDone)
            {
                long itemDownloadedSize = (long)(handle.PercentComplete * itemSize);
                NotifyUpdateProgress(totalDownloadedSize + itemDownloadedSize, needUpdateSize);
                yield return null;
                if(handle.Status == AsyncOperationStatus.Failed)
                {
                    downloadFaild = true;
                    break;
                }
            }


            if (downloadFaild)
            {
                OnDownloadFaild();
                break;
            }

            totalDownloadedSize += itemSize;
            NotifyUpdateProgress(totalDownloadedSize, needUpdateSize);
        }

        if (!downloadFaild)
        {
            OnUpdateComplete();
        }
    }

    private void OnDownloadFaild()
    {
        Debug.Log("Download Faild");
        if (MoeAssetUpdateUI.GetInstance() != null)
        {
            MoeAssetUpdateUI.Inst.OnDownloadFaild();
        }
    }

    private void OnUpdateComplete()
    {
        Debug.Log("Update Complete");
        onDoneCallback?.Invoke();
        if(MoeAssetUpdateUI.GetInstance() != null)
        {
            MoeAssetUpdateUI.Inst.OnUpdateComplete();
        }
    }

    private void NotifyUpdateProgress(long downloadedSize, long needDownloadSize)
    {
        if (MoeAssetUpdateUI.GetInstance() != null)
        {
            MoeAssetUpdateUI.Inst.UpdateProgress(downloadedSize, needDownloadSize);
        }
    }
}