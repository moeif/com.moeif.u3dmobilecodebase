using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public enum EnUserDataResult
{
    None,
    OK,
    NetworkError,
    NoData,
}

public class OptionUserData<T> where T : UserDataBase
{
    public EnUserDataResult result;
    public T data;


    public OptionUserData(EnUserDataResult result, T data)
    {
        this.result = result;
        this.data = data;
    }
}

public class OptionUserNetData
{
    public EnUserDataResult result;
    public string jsonData;

    public OptionUserNetData(EnUserDataResult result, string jsonStr)
    {
        this.result = result;
        this.jsonData = jsonStr;
    }
}

public class UserDataBase
{
    public static T LoadOrCreateLocalData<T>(string dataKey) where T : UserDataBase
    {
        string jsonStr = MoeUtils.LoadStrDataFromLocal(dataKey);
        if (!string.IsNullOrEmpty(jsonStr))
        {
            T data = MoeUtils.FromJson<T>(jsonStr);

            if (data == null)
            {
                data = System.Activator.CreateInstance<T>();
            }

            return data;
        }
        else
        {
            return System.Activator.CreateInstance<T>();
        }
    }

    public static T LoadLocalData<T>(string dataKey) where T : UserDataBase
    {
        string jsonStr = MoeUtils.LoadStrDataFromLocal(dataKey);
        if (!string.IsNullOrEmpty(jsonStr))
        {
            T data;
            try
            {
                data = MoeUtils.FromJson<T>(jsonStr);
                return data;
            }
            catch
            {
                Debug.LogErrorFormat("数据反序列化失败, dataKey: {0}", dataKey);
                return null;
            }
        }
        else
        {
            return null;
        }
    }


    public virtual void SaveInstanceToLocal(string dataKey)
    {
        if (!string.IsNullOrEmpty(dataKey))
        {
            string jsonStr = MoeUtils.ToJson(this);
            PlayerPrefs.SetString(dataKey, jsonStr);
        }
        else
        {
            Debug.LogErrorFormat("DataKey is Empty!");
        }
    }

    public static void SaveJsonToLocal(string dataKey, string jsonStr)
    {
        if (!string.IsNullOrEmpty(dataKey))
        {
            PlayerPrefs.SetString(dataKey, jsonStr);
        }
        else
        {
            Debug.LogErrorFormat("DataKey is Empty!");
        }
    }

}