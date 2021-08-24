using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System;

public class MoeFileSyncConfig
{
    public string InProjectDir;
    public string OutProjectDir;
    public string SyncType;
    public bool In2Out;
    public bool Out2In;
}

public class MoeFileSync : EditorWindow
{
    enum SyncType
    {
        In2Out,
        Out2In,
    }

    [MenuItem("MoeTools/FileSync")]
    static void DoIt()
    {
        MoeFileSync window = (MoeFileSync)EditorWindow.GetWindow(typeof(MoeFileSync));
        window.Show();
        window.LoadSyncConfigList();
    }

    private List<MoeFileSyncConfig> syncConfigList = new List<MoeFileSyncConfig>();

    private void LoadSyncConfigList()
    {
        string configFile = Path.Combine(Application.dataPath, "AppConfigs/MoeFileSyncConfig.json");
        if (File.Exists(configFile))
        {
            string text = File.ReadAllText(configFile, System.Text.Encoding.UTF8);
            syncConfigList = JsonConvert.DeserializeObject<List<MoeFileSyncConfig>>(text);
        }
        else
        {
            Debug.LogErrorFormat("未找到同步配置文件: {0}", configFile);
        }
    }

    //static string InPorjectBakPath = Application.dataPath + "/InSyncBak/";

    private void OnGUI()
    {
        GUILayout.Space(20);
        if (syncConfigList != null)
        {
            foreach (MoeFileSyncConfig syncConfig in syncConfigList)
            {
                GUILayout.Label("InProjectDir: " + syncConfig.InProjectDir);
                GUILayout.Label("OutProjectDir: " + syncConfig.OutProjectDir);
                GUILayout.BeginHorizontal();
                if (syncConfig.In2Out)
                {
                    if (GUILayout.Button("In -> Out"))
                    {
                        if (EditorUtility.DisplayDialog("", "确定执行同步 In -> Out", "确定", "取消"))
                        {
                            SyncFile(syncConfig, SyncType.In2Out);
                        }
                        else
                        {
                            Debug.LogFormat("同步操作已取消");
                        }
                    }
                }

                if (syncConfig.Out2In)
                {
                    if (GUILayout.Button("Out -> In"))
                    {
                        if (EditorUtility.DisplayDialog("", "确定执行同步 Out -> In", "确定", "取消"))
                        {
                            SyncFile(syncConfig, SyncType.Out2In);
                        }
                        else
                        {
                            Debug.LogFormat("同步操作已取消");
                        }
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(30);
            }
        }
    }

    private void SyncFile(MoeFileSyncConfig syncConfig, SyncType syncType)
    {
        Debug.LogFormat("执行同步: {0}", syncType);
        string fromPath = syncType == SyncType.In2Out ? syncConfig.InProjectDir : syncConfig.OutProjectDir;
        string toPath = syncType == SyncType.In2Out ? syncConfig.OutProjectDir : syncConfig.InProjectDir;

        Dictionary<string, List<string>> fromFilesDict = GetFilesOfFolder(fromPath);
        Dictionary<string, List<string>> toFileDict = GetFilesOfFolder(toPath);
        DoSync(syncConfig, fromPath, toPath, fromFilesDict, toFileDict);
    }

    private void DoSync(MoeFileSyncConfig syncConfig, string fromRoot, string toRoot, Dictionary<string, List<string>> fromDict, Dictionary<string, List<string>> toDict)
    {
        if(syncConfig.SyncType == "Replace")
        {
            if (Directory.Exists(toRoot))
            {
                Directory.Delete(toRoot, true);
            }
            var enumer = fromDict.GetEnumerator();
            while (enumer.MoveNext())
            {
                string relativeDir = enumer.Current.Key;
                string toFullDir = Path.Combine(toRoot, relativeDir);
                if (!Directory.Exists(toFullDir))
                {
                    Directory.CreateDirectory(toFullDir);
                }

                string fromFullDir = Path.Combine(fromRoot, relativeDir);
                foreach(string file in enumer.Current.Value)
                {
                    string fromFilePath = Path.Combine(fromFullDir, file);
                    string toFilePath = Path.Combine(toFullDir, file);
                    File.Copy(fromFilePath, toFilePath);
                    Debug.LogFormat("{0} -> {1}", fromFilePath, toFilePath);
                }
            }
        }
    }

    private Dictionary<string, List<string>> GetFilesOfFolder(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return null;
        }
        string[] files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
        Dictionary<string, List<string>> dirWithFileDict = new Dictionary<string, List<string>>();

        foreach(string file in files)
        {
            if (!string.IsNullOrEmpty(file))
            {
                string extension = Path.GetExtension(file);
                if(extension == ".meta")
                {
                    continue;
                }

                string dir = Path.GetDirectoryName(file).Replace(rootPath, "");
                string fileName = Path.GetFileName(file);

                if (!dirWithFileDict.ContainsKey(dir))
                {
                    dirWithFileDict.Add(dir, new List<string>());
                }
                dirWithFileDict[dir].Add(fileName);
            }
        }

        return dirWithFileDict;
    }


    /// <summary>
    /// 获取文件MD5值
    /// </summary>
    /// <param name="fileName">文件绝对路径</param>
    /// <returns>MD5值</returns>
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

}