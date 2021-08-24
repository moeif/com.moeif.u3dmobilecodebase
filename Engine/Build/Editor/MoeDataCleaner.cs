using System.Collections;
using UnityEngine;
using UnityEditor;

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
    }

}