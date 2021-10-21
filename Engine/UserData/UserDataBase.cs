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
        T data = LoadLocalData<T>(dataKey);
        if (data == null)
        {
            return System.Activator.CreateInstance<T>();
        }
        else
        {
            return data;
        }
    }

    public static T CreateInstance<T>() where T : UserDataBase
    {
        T data = System.Activator.CreateInstance<T>();
        data.OnInstanceCreated();
        return data;
    }

    public virtual void OnInstanceCreated()
    {

    }

    public static T LoadLocalData<T>(string dataKey) where T : UserDataBase
    {
        T data = MoeLocalSave.Load<T>(dataKey);
        if (data != null)
        {
            return data;
        }
        else
        {
            Debug.LogFormat("本地数据加载失败: {0}", dataKey);
        }

        return null;


    }

}