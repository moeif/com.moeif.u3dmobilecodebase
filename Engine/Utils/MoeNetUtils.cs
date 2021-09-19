using BestHTTP;
using UnityEngine;
using System.Threading.Tasks;

public static class MoeNetUtils
{
    static MoeNetUtils()
    {
        HTTPManager.KeepAliveDefaultValue = false;
    }

    public static async Task<string> Get(string url, int tryCount = 1)
    {
        if (string.IsNullOrEmpty(url) || tryCount < 1)
        {
            Debug.LogErrorFormat("URL is null or tryCount < 1; URL: {0}, tryCount: {1}", url, tryCount);
            return null;
        }

        string result = null;
        for (int i = 0; i < tryCount; ++i)
        {
            Debug.LogFormat("GET 网络请求: {0}, tryCount: {1}, now: {2}", url, tryCount, i + 1);
            try
            {
                var req = new HTTPRequest(new System.Uri(url), methodType: HTTPMethods.Get);
                result = await req.GetAsStringAsync();
            }
            catch
            {
                Debug.LogErrorFormat("Net Get Request Failed: {0}, ReqIndex: {1}", url, i + 1);
            }

            if (!string.IsNullOrEmpty(result))
            {
                break;
            }
        }

        return result;
    }

}