using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UniRx.Async;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MoeResManager : MoeSingleton<MoeResManager>
{
    private Dictionary<string, Object> preloadedAssetDict = new Dictionary<string, Object>();

    public void PreloadAssets(string[] assetsAddress, System.Action onDoneCallback)
    {
        if(assetsAddress != null && assetsAddress.Length > 0)
        {
            // TODO: Preload assets
        }

        onDoneCallback?.Invoke();
    }

    public Object GetAsset(string assetAddress)
    {
        Object obj = null;
        preloadedAssetDict.TryGetValue(assetAddress, out obj);
        return obj;
    }

    public void LoadAssetAsync<T>(string address, System.Action<T> callback)
    {
        Addressables.LoadAssetsAsync<T>(address, callback);
    }

    public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
    {
        T asset = await Addressables.LoadAssetAsync<T>(address).Task;
        return asset;
    }
}