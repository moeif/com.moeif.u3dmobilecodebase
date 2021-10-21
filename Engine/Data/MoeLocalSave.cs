using UnityEngine;

public static class MoeLocalSave
{
    public static ES3Settings es3Setting = new ES3Settings(ES3.EncryptionType.AES, AppConfig.Inst.CustomKey);
    const string DATA_FILE = "_data";


    static string PATH
    {
        get
        {
            return System.IO.Path.Combine(Application.persistentDataPath, DATA_FILE);
        }
    }

    public static T Load<T>(string key)
    {
        Debug.LogFormat("LocalSave 加载数据: {0}", key);
        try
        {
            T data = ES3.Load<T>(key, PATH, es3Setting);
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("MoeLocalSave Load Exception: {0}", e.ToString());
            return default(T);
        }
    }

    public static void Save<T>(string key, T value)
    {
        Debug.LogFormat("LocalSave 保存数据: {0}", key);
        try
        {
            ES3.Save(key, value, PATH, es3Setting);
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("MoeLocalSave Save Exception: {0}", e.ToString());
        }
    }

    public static void DeleteData()
    {
        ES3.DeleteFile(PATH);
    }
}