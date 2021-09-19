using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class EngineSync
{
    [MenuItem("MoeTools/ProjectEngine->Codebase")]
    static void EngineOut()
    {
        string codeBasePath = "/Users/fredshao/moeifstudio/codebase/Engine/";
        if (Directory.Exists(codeBasePath))
        {
            Directory.Delete(codeBasePath, true);
        }
        Directory.CreateDirectory(codeBasePath);

        string projectEnginePath = Path.Combine(Application.dataPath, "ThirdParty/Engine/");

        DirectoryCopy(projectEnginePath, codeBasePath, true);
    }

    [MenuItem("MoeTools/Codebase->ProjectEngine")]
    static void EngineIn()
    {
        string codeBasePath = "/Users/fredshao/moeifstudio/codebase/Engine/";
        string projectEnginePath = Path.Combine(Application.dataPath, "ThirdParty/Engine/");
        Directory.Delete(projectEnginePath);

        DirectoryCopy(codeBasePath, projectEnginePath, true);
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".meta")
            {
                continue;
            }
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
}
