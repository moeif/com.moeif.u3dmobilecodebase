using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using global::FlatBuffers;
public class IMoeConfigTableBase
{
    public virtual async Task LoadConfigTable()
    {

    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">具体数据类</typeparam>
/// <typeparam name="U">Flatbuffers 类型</typeparam>
/// <typeparam name="V">逻辑数据类型</typeparam>
public class MoeConfigTable<T, V> : IMoeConfigTableBase where T : MoeConfigTable<T, V>
{
    private static T _inst = null;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                // GameObject obj = new GameObject(typeof(T).Name);
                // DontDestroyOnLoad(obj);
                // _inst = obj.AddComponent<T>();
                var type = typeof(T);
                _inst = System.Activator.CreateInstance<T>();
                Debug.LogFormat("{0} Created!", typeof(T).Name);
            }
            return _inst;
        }
    }

    public MoeConfigTable()
    {
        _inst = this as T;
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

    public override async Task LoadConfigTable()
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

    public Vector2 ParseVector2(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            string[] strArray = str.Split(',');
            if (strArray != null && strArray.Length > 0)
            {
                List<float> iDataList = new List<float>();
                foreach (string strData in strArray)
                {
                    float faceId = -1;
                    if (float.TryParse(strData, out faceId))
                    {
                        iDataList.Add(faceId);
                    }
                }
                return new Vector2(iDataList[0], iDataList[1]);
            }
        }

        return Vector2.zero;
    }

}