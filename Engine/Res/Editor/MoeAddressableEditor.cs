using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MoeAddressableEditor : ScriptableObject
{
    [MenuItem("MoeTools/重设所有资源Address")]
    static void DoIt()
    {
        //string path = "Assets/Res/UI/UIMainPanel.prefab";
        //Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

        //AddressableHelper.CreateAssetEntry<Object>(asset, "UI", "UIMainPanel");
        //AssetDatabase.Refresh();

        Dictionary<string,string> assetAddressDict = GetResPathAddressInfo();

        var enumer = assetAddressDict.GetEnumerator();
        while (enumer.MoveNext())
        {
            string assetPath = enumer.Current.Key;
            string address = enumer.Current.Value;
            SetAssetAddress(assetPath, address);
        }

        AssetDatabase.Refresh();
    }

    private static void SetAssetAddress(string assetPath, string address)
    {
        string groupName = address.Replace(string.Format("/{0}", Path.GetFileName(address)), "").Replace('/', '-');
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        AddressableHelper.CreateAssetEntry<Object>(asset, groupName, address);
    }

    private static Dictionary<string, string> GetResPathAddressInfo()
    {
        string rootPath = Path.Combine(Application.dataPath, "Res");
        DirectoryInfo rootDirInfo = new DirectoryInfo(rootPath);

        Dictionary<string, string> assetPath2AddressDict = new Dictionary<string, string>();

        string resPath = rootPath.Replace('\\', '/') + "/";

        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories("*", SearchOption.AllDirectories))
        {
            Debug.LogFormat("{0} - {1}", dirInfo.Name, dirInfo.FullName);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".meta")
                {
                    continue;
                }
                string simpleName = Path.GetFileNameWithoutExtension(file.FullName);
                string filePath = Path.Combine(dirInfo.FullName, simpleName);
                filePath = filePath.Replace('\\', '/');

                string address = filePath.Replace(resPath, "");
                string assetPath = string.Format("Assets/Res/{0}{1}", address, file.Extension);
                assetPath2AddressDict.Add(assetPath, address);

                
                Debug.LogFormat("    {0} - {1} - {2}", simpleName, address, assetPath);
            }
        }

        return assetPath2AddressDict;
    }

}