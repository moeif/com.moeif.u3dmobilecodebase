using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using global::FlatBuffers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">具体数据类</typeparam>
/// <typeparam name="U">Flatbuffers 类型</typeparam>
/// <typeparam name="V">逻辑数据类型</typeparam>
public class MoeConfigTable<T, V> : MonoBehaviour where T : MoeConfigTable<T, V>
{
    private static T _inst = null;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(obj);
                _inst = obj.AddComponent<T>();
            }
            return _inst;
        }
    }

    public List<V> dataList = new List<V>();
    protected Dictionary<int, V> dataDict = new Dictionary<int, V>();
    private T config = null;


    protected virtual string tableName
    {
        get
        {
            return "UnOverride";
        }
    }

    public void GetInstance()
    {

    }

    public V Get(int id)
    {
        if (dataDict.ContainsKey(id))
        {
            return dataDict[id];
        }
        return default(V);
    }

    protected void Add(int id, V item)
    {
        if (!dataDict.ContainsKey(id))
        {
            dataDict.Add(id, item);
            dataList.Add(item);
        }
        else
        {
            Debug.LogErrorFormat("Duplicate {0} config id: {1}", item.GetType(), id);
        }
    }

    public async Task LoadConfigTable()
    {
        TextAsset result = await Addressables.LoadAssetAsync<TextAsset>("ConfigBytes/" + tableName).Task;
        ByteBuffer buffer = new ByteBuffer(result.bytes);
        OnDataLoaded(buffer);
        Debug.LogFormat("Table {0} Loaded!", tableName);
    }


    protected virtual void OnDataLoaded(ByteBuffer buffer)
    {

    }


    protected int[] ParseIntArray(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            string[] strArray = str.Split(',');
            if (strArray != null && strArray.Length > 0)
            {
                List<int> iDataList = new List<int>();
                foreach (string strData in strArray)
                {
                    int faceId = -1;
                    if (int.TryParse(strData, out faceId))
                    {
                        iDataList.Add(faceId);
                    }
                }
                return iDataList.ToArray();
            }
        }

        return null;
    }

}