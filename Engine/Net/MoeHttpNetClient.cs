using System.Collections;
using UnityEngine;
using UniRx.Async;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using BestHTTP;

public class MoeHttpNetClient : MoeSingleton<MoeHttpNetClient>
{
    protected override void InitOnCreate()
    {
        HTTPManager.KeepAliveDefaultValue = false;
    }

    public async UniTask<T> Get<T>(string url)
    {
        try
        {
            var request = new HTTPRequest(new System.Uri(url), methodType: HTTPMethods.Get);
            var response = await request.GetAsStringAsync();
            T npr = JsonConvert.DeserializeObject<T>(response);
            return npr;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("反序列化网络数据失败: {0}", e.ToString());
            return default(T);
        }
    }


    public async UniTask<R> Post<U, R>(string url, U postInfo)
    {
        try
        {
            string jsonStr = JsonConvert.SerializeObject(postInfo);
            var request = new HTTPRequest(new System.Uri(url), methodType: HTTPMethods.Put);
            request.RawData = System.Text.Encoding.UTF8.GetBytes(jsonStr);
            request.AddHeader("Content-Type", "application/json");
            var response = await request.GetAsStringAsync();
            R npr = JsonConvert.DeserializeObject<R>(response);
            return npr;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogErrorFormat("反序列化网络数据失败: {0}", e.ToString());
            return default(R);
        }
    }


}