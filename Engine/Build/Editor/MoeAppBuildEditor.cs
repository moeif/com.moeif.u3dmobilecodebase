using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System;

public class EditorAppConfigCommonInfo
{
    public string BundleName;
    public string CompanyName;
    public Dictionary<string, string> ProductNames;
    public string CustomKey;
}

public class EditorAppConfigChannelInfo
{
    public string BundleName;
    public string Language;
    public string ChannelName;
    public string TargetPath;
    public bool Privacy;
    public bool SignV1;
    public string MarketUrl;
}

public class EditorAppConfigPlatforms
{
    public List<EditorAppConfigChannelInfo> Android;
    public List<EditorAppConfigChannelInfo> Win;
    public List<EditorAppConfigChannelInfo> iOS;
    public List<EditorAppConfigChannelInfo> OSX;
}


public class EditorAppConfig
{
    public EditorAppConfigCommonInfo CommonInfo;
    public EditorAppConfigPlatforms Platforms;
}


public class MoeAppBuildEditor : EditorWindow
{
    private static EditorAppConfig eAppConfig = null;

    [MenuItem("Build/BuildEditor")]
    static void DoIt()
    {
        MoeAppBuildEditor window = (MoeAppBuildEditor)EditorWindow.GetWindow(typeof(MoeAppBuildEditor));
        window.Show();
        versionStr = LoadLastVersion();
        eAppConfig = LoadAppConfigs();
        SetAdmobAppId();
    }

    private static string LoadLastVersion()
    {
#if UNITY_ANDROID
        string path = Path.Combine(Application.dataPath, "AppConfigs/androidLastVersion.txt");
#elif UNITY_IPHONE
        string path = Path.Combine(Application.dataPath, "AppConfigs/iosLastVersion.txt");
#endif
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
            return text;
        }
        else
        {
            return string.Empty;
        }
    }

    private static void SaveLastVersion(string versionText)
    {
#if UNITY_ANDROID
        string path = Path.Combine(Application.dataPath, "AppConfigs/androidLastVersion.txt");
#elif UNITY_IPHONE
        string path = Path.Combine(Application.dataPath, "AppConfigs/iosLastVersion.txt");
#endif
        File.WriteAllText(path, versionText);
    }


    private static EditorAppConfig LoadAppConfigs()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "AppConfigs/AppConfig.json");
        string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
        EditorAppConfig jsonObj = JsonConvert.DeserializeObject<EditorAppConfig>(text);
        return jsonObj;
    }

    const string AdmobAndroid = "ca-app-pub-3110170387295963~9585047413";
    const string AdmobiOS = "ca-app-pub-3110170387295963~8009808530";

    private static void SetAdmobAppId()
    {
#if Admob
        GoogleMobileAds.Editor.GoogleMobileAdsSettingsEditor.SetAppId(AdmobiOS, AdmobAndroid);
#endif
    }


    private static string versionStr;
    public const int mainVersion = 1;
    public const int subVersion = 0;
    public int svnVersion = -1;
    public string androidBuildVersionCodeStr;

    public string workingChannelInfo = string.Empty;

    private void OnGUI()
    {
        if (eAppConfig == null)
        {
            GUILayout.Label("配置文件为空");
            return;
        }

        GUILayout.Space(30);
        GUILayout.Label("版本号: ");
        versionStr = GUILayout.TextField(versionStr);
        GUILayout.Space(10);
        //if (GUILayout.Button("RefreshVersion"))
        //{
        //    SetVersion();
        //}

        GUILayout.Label("Android Build VersionCode:");
        androidBuildVersionCodeStr = GUILayout.TextField(androidBuildVersionCodeStr);

        GUILayout.Space(20);
        GUILayout.Label(workingChannelInfo);

        GUILayout.Space(50);
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Android");
        if (eAppConfig.Platforms.Android != null)
        {
            for (int i = 0; i < eAppConfig.Platforms.Android.Count; ++i)
            {
                EditorAppConfigChannelInfo eChannelInfo = eAppConfig.Platforms.Android[i];
                if (GUILayout.Button(eChannelInfo.ChannelName))
                {
                    Debug.Log("编译渠道包: " + eChannelInfo.ChannelName);
                    BuildTarget(UnityEditor.BuildTarget.Android, eChannelInfo.ChannelName);
                }
            }
        }

        GUILayout.Space(20);

        if (eAppConfig.Platforms.iOS != null)
        {
            for (int i = 0; i < eAppConfig.Platforms.iOS.Count; ++i)
            {
                EditorAppConfigChannelInfo eChannelInfo = eAppConfig.Platforms.iOS[i];
                if (GUILayout.Button(eChannelInfo.ChannelName))
                {
                    BuildTarget(UnityEditor.BuildTarget.iOS, eChannelInfo.ChannelName);
                }
            }
        }
        EditorGUILayout.EndVertical();


    }


    private void BuildTarget(BuildTarget buildTarget, string channelName)
    {
        // 尝试删除临时目录
        string tmpGradleDir = Application.dataPath + "/../Temp/gradleOut";
        if (Directory.Exists(tmpGradleDir))
        {
            Directory.Delete(tmpGradleDir, true);
        }

        // generate target platform appconfig json
        if (string.IsNullOrEmpty(versionStr))
        {
            Debug.LogError("请输入版本号");
            return;
        }

        SaveLastVersion(versionStr);
        string configFileName = string.Empty;

        BuildTargetGroup btg = BuildTargetGroup.Android;
        if (buildTarget == UnityEditor.BuildTarget.Android)
        {
            btg = BuildTargetGroup.Android;
            configFileName = AppConfig.ANDROID_FILE_NAME;
        }
        else if (buildTarget == UnityEditor.BuildTarget.iOS)
        {
            btg = BuildTargetGroup.iOS;
            configFileName = AppConfig.iOS_FILE_NAME;
        }

        EditorUserBuildSettings.SwitchActiveBuildTarget(btg, buildTarget);
        AssetDatabase.Refresh();

        EditorAppConfigChannelInfo channelInfo = GetChannelInfo(buildTarget, channelName);
        AppConfig appConfig = new AppConfig();
        appConfig.BundleName = string.IsNullOrEmpty(channelInfo.BundleName) ? eAppConfig.CommonInfo.BundleName : channelInfo.BundleName;
        appConfig.ProductName = eAppConfig.CommonInfo.ProductNames[channelInfo.Language];
        appConfig.Lang = channelInfo.Language;
        appConfig.ChannelName = channelName;
        appConfig.Privacy = channelInfo.Privacy;
        appConfig.VERSION = versionStr;
        appConfig.CustomKey = eAppConfig.CommonInfo.CustomKey;
        appConfig.WriteConfig(configFileName);

        AssetDatabase.Refresh();

        PlayerSettings.productName = appConfig.ProductName;
        PlayerSettings.applicationIdentifier = appConfig.BundleName;
        PlayerSettings.bundleVersion = versionStr;

        if (buildTarget == UnityEditor.BuildTarget.Android)
        {
            int bundleVersionCode = 0;
            if (!int.TryParse(androidBuildVersionCodeStr, out bundleVersionCode))
            {
                Debug.LogFormat("Bundle Version Code 有问题！");
                return;
            }
            else
            {
                if (bundleVersionCode < 1)
                {
                    Debug.LogFormat("Bundle Version Code 有问题！");
                    return;
                }
            }

            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }


        workingChannelInfo = string.Format("正在编译渠道: {0}", appConfig.ChannelName);

        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();

        string[] bundleNameArray = appConfig.BundleName.Split('.');
        string targetFilePrefixName = bundleNameArray[bundleNameArray.Length - 1];

        EditorUserBuildSettings.buildAppBundle = appConfig.ChannelName == "GooglePlay" ? true : false;

        if (buildTarget == UnityEditor.BuildTarget.Android)
        {
            //string[] versionStrArray = versionStr.Split('.');
            string projectPath = Application.dataPath.Replace("/Assets", "");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = "/Users/fredshao/onedrive/Moeif/Moeif/AndroidKeystore/moeif.jks";
            PlayerSettings.Android.keystorePass = "962464";
            PlayerSettings.Android.keyaliasName = "moeif";
            PlayerSettings.Android.keyaliasPass = "962464";

            string targetDir = projectPath + "/AndroidAPK/";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            string targetName = string.Format("{0}_{1}_{2}.apk", targetFilePrefixName, appConfig.ChannelName, versionStr);
            UnityEditor.Build.Reporting.BuildReport ret = BuildPipeline.BuildPlayer(GetBuildScenes(), targetDir + targetName, buildTarget, BuildOptions.None);
            EditorUtility.RevealInFinder(targetDir);
        }
        else if (buildTarget == UnityEditor.BuildTarget.iOS)
        {
            UnityEditor.Build.Reporting.BuildReport ret = BuildPipeline.BuildPlayer(GetBuildScenes(), channelInfo.TargetPath, buildTarget, BuildOptions.None);
        }

        // 如果要拷贝到某个位置
        //if (!string.IsNullOrEmpty(channelInfo.APKTargetPath))
        //{
        //CopyTo(targetDir, channelInfo.APKTargetPath, targetName);
        //EditorUtility.RevealInFinder(channelInfo.APKTargetPath);
        //}

        workingChannelInfo = string.Empty;
    }

    public static void ResignV1(string apkPath, string signedApkPath, string keyStorePath, string keyStorePass)
    {
        string v1SignerPath = @"C:\Program Files\Unity\Hub\Editor\2020.2.1f1c1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\build-tools\28.0.3\";
    }

    private void CopyTo(string fromDir, string toDir, string fileName)
    {
        if (!Directory.Exists(toDir))
        {
            Directory.CreateDirectory(toDir);
        }

        string fromPath = System.IO.Path.Combine(fromDir, fileName);
        string toPath = System.IO.Path.Combine(toDir, fileName);
        File.Copy(fromPath, toPath, true);
    }

    private string[] GetBuildScenes()
    {
        List<string> sceneList = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            sceneList.Add(scene.path);
        }

        return sceneList.ToArray();
    }

    private EditorAppConfigChannelInfo GetChannelInfo(BuildTarget buildTarget, string channelName)
    {
        List<EditorAppConfigChannelInfo> channelInfoList = null;
        if (buildTarget == UnityEditor.BuildTarget.Android)
        {
            channelInfoList = eAppConfig.Platforms.Android;
        }
        else if (buildTarget == UnityEditor.BuildTarget.iOS)
        {
            channelInfoList = eAppConfig.Platforms.iOS;
        }

        if (channelInfoList != null)
        {
            foreach (EditorAppConfigChannelInfo info in channelInfoList)
            {
                if (info.ChannelName == channelName)
                {
                    return info;
                }
            }
        }

        return null;
    }

    private void SetVersion()
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = "cmd.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.Start();

        p.StandardInput.WriteLine("svn info --revision HEAD & exit");
        p.StandardInput.AutoFlush = true;
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        p.Close();

        foreach (string line in output.Split('\n'))
        {
            if (line.StartsWith("Revision: "))
            {
                string versionStr = line.Split(' ')[1].Trim();
                svnVersion = int.Parse(versionStr);
                break;
            }
        }


        versionStr = string.Format("{0}.{1}.{2}", mainVersion, subVersion, svnVersion);

    }
}