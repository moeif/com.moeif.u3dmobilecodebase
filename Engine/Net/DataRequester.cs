using System.Collections;
using UnityEngine;
using UniRx.Async;
using UnityEngine.Networking;

public class DataRequester : MoeSingleton<DataRequester> 
{
    public async UniTask<string> RequestData(string url)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        await req.SendWebRequest();
        if(req.result == UnityWebRequest.Result.Success)
        {
            return req.downloadHandler.text;
        }
        return null;
    }
}