using System.Collections;
using UnityEngine;

public class PackageHandleMap : MonoBehaviour
{
    private static PackageHandleMap _inst = null;

    public static PackageHandleMap Inst
    {
        get
        {
            if(_inst == null)
            {
                Debug.LogErrorFormat("未具体实现PackageHandleMap!");
                CreateInstance<PackageHandleMap>();
            }
            return _inst;
        }
    }

    public static void CreateInstance<T>() where T : PackageHandleMap
    {
        GameObject obj = new GameObject("PackageHandleMap");
        DontDestroyOnLoad(obj);
        _inst = obj.AddComponent<T>();
        _inst.OnCreateInstance();
    }

    public void HandlePackage(int protoId, byte[] packageData)
    {
        OnHandlePackage(protoId, packageData);
    }

    protected virtual void OnCreateInstance()
    {

    }

    protected virtual void OnHandlePackage(int protoId, byte[] packageData)
    {

    }
    
}