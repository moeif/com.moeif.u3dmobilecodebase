using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;
using FlatBuffers;

public class MoeDataCleaner
{
    [MenuItem("Build/CleanSavedData")]
    public static void CleanSavedData()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "SaveFile.es3");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }

        DirectoryInfo dInfo = new DirectoryInfo("./");
        Debug.LogFormat("Dir: {0}", dInfo.FullName);
    }



}