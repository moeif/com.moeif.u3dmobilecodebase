//using UnityEngine;
//using System.Collections;
//using System.IO;
//using LitJson;
//using UnityEditor;

//public class AppConfigGenerator
//{
//    [MenuItem("MoeTools/GenerateAppConfig")]
//    public static void Init()
//    {
//        string path = System.IO.Path.Combine(Application.dataPath, "AppConfigs/AppConfig.json");
//        EditorAppConfigParser parse = new EditorAppConfigParser(path);

//        string outPath = Path.Combine(Application.dataPath, "Resources/AppConfig.json");

//        parse.GenerateBuildConfigFile("Android", "GooglePlay", outPath);
//    }
//}

//public class EditorAppConfigParser
//{
//    public string configFilePath { get; private set; }
//    private JsonData jsonData;
//    public EditorAppConfigParser(string configFilePath)
//    {
//        this.configFilePath = configFilePath;
//        string text = File.ReadAllText(configFilePath);
//        jsonData = JsonMapper.ToObject(text);
//    }

//    public bool GenerateBuildConfigFile(string platformStr, string channelStr, string configOutPath)
//    {
//        AppConfig appConfig = new AppConfig();
//        appConfig.BundleName = string.IsNullOrEmpty(platformStr) ? (string)jsonData["CommonInfo"]["BundleName"] : (string)jsonData["Platforms"][platformStr][channelStr]["BundleName"];
//        appConfig.ChannelName = channelStr;
//        //appConfig.URL_NICKNAME_ZH = (string)jsonData["CommonInfo"]["DataServer"]["URL_GET_NICKNAME_ZH"];
//        //appConfig.URL_NICKNAME_EN = (string)jsonData["CommonInfo"]["DataServer"]["URL_GET_NICKNAME_EN"];
//        //appConfig.URL_RANK_LIST = (string)jsonData["CommonInfo"]["DataServer"]["URL_GET_RANKLIST"];
//        //appConfig.URL_MAX_RANK_SCORE = (string)jsonData["CommonInfo"]["DataServer"]["URL_GET_MAX_RANK_SCORE"];
//        //appConfig.URL_UPDATE_SOCRE = (string)jsonData["CommonInfo"]["DataServer"]["URL_UPDATE_SCORE"];
//        appConfig.WriteConfig();
//        AssetDatabase.Refresh();
//        return true;
//    }


    
//}