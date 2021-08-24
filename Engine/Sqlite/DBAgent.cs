using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using SQLite;
using UniRx.Async;


public class DBAgent
{
    private SQLiteAsyncConnection _connection;
    private string dbPath = string.Empty;

    public DBAgent(string dbName)
    {
        // 如果StreamingAsset里有DB，并且PersistentDataPath里无DB，则要拷贝一份StreamingAsset里的Db到PersistentPath里
        CopyFromStreamingAssetToPersistentAsset(dbName);
        dbPath = string.Format("{0}/{1}", Application.persistentDataPath, dbName);

        _connection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
    }
   
    private void CopyFromStreamingAssetToPersistentAsset(string dbName)
    {
        string filePath = string.Format("{0}/{1}", Application.persistentDataPath, dbName);
        if (!File.Exists(filePath))
        {
#if UNITY_EDITOR
            string fromPath = string.Format("{0}/{1}", Application.streamingAssetsPath, dbName);
            if (File.Exists(fromPath))
            {
                File.Copy(fromPath, filePath);
            }

#elif UNITY_ANDROID
            //var loadDB = UnityWebRequest.Get("jar:file://" + Application.dataPath + "!/assets/" + dbName);
            //var loadDB = UnityWebRequest.Get(string.Format("{0}/{1}", Application.streamingAssetsPath, dbName));
            var loadDB = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, dbName));
            loadDB.SendWebRequest();
            while (!loadDB.isDone)
            {
                if(loadDB.result == UnityWebRequest.Result.ConnectionError ||
                    loadDB.result == UnityWebRequest.Result.DataProcessingError ||
                    loadDB.result == UnityWebRequest.Result.ProtocolError)
                {
                    break;
                }

                //Debug.LogFormat("result: {0}  isDown: {1}  progress: {2}", loadDB.result, loadDB.isDone, loadDB.downloadProgress);
            }

            if(loadDB.result == UnityWebRequest.Result.Success)
            {
                if (loadDB.downloadHandler.data != null)
                {
                    File.WriteAllBytes(filePath, loadDB.downloadHandler.data);
                }
            }
            else
            {
                Debug.LogErrorFormat("读取错误: {0}", loadDB.result);
            }
#elif UNITY_IPHONE
            var loadDB = Application.dataPath + "/Raw/" + dbName;
            if (File.Exists(loadDB))
            {
                Debug.LogFormat("拷贝数据库: {0} -> {1}", loadDB, filePath);
                File.Copy(loadDB, filePath);
            }
#endif


        }
    }

    public async UniTask<CreateTableResult> CreateTable<T>() where T : new()
    {
        // 创建表
        return await _connection.CreateTableAsync<T>();
    }


    public void Close()
    {
        _connection.CloseAsync();
    }

    public async UniTask InsertOrUpdateData<T>(T dataInstance) where T: new()
    {
        await _connection.InsertOrReplaceAsync(dataInstance);
    }

    public async void InsertOrUpdateData<T>(T dataInstance, System.Action<int> callback) where T: new()
    {
        int resultCount = await _connection.InsertOrReplaceAsync(dataInstance);
        callback?.Invoke(resultCount);
    }

    public async void InsertDataList<T>(IEnumerable<T> dataList) where T: new()
    {
        await _connection.InsertAllAsync(dataList);
    }

  
    public async void GetLimitData<T>(int count, System.Action<List<T>> callback) where T : new()
    {
        string query = string.Empty;
        if (count > 0)
        {
            query = string.Format("SELECT * FROM {0} ORDER BY id DESC LIMIT {1}", typeof(T).Name, count);
        }
        else
        {
            query = string.Format("SELECT * FROM {0} ORDER BY id DESC", typeof(T).Name);
        }

        var result = await _connection.QueryAsync<T>(query);
        callback?.Invoke(result);
    }

    public async UniTask<List<T>> GetLimitData<T>(int count = -1) where T : new()
    {
        string query = string.Empty;
        if (count > 0)
        {
            query = string.Format("SELECT * FROM {0} ORDER BY id DESC LIMIT {1}", typeof(T).Name, count);
        }
        else
        {
            query = string.Format("SELECT * FROM {0} ORDER BY id DESC", typeof(T).Name);
        }

        var result = await _connection.QueryAsync<T>(query);
        return result;
    }

    public async void DeleteData<T>(int id)
    {
        await _connection.DeleteAsync<T>(id);
    }




}
