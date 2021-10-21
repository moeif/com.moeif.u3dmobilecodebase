using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class AppConfig
{
    private static AppConfig _inst = null;
    public static AppConfig Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = LoadConfig();
            }
            return _inst;
        }
    }

    public const string iOS_FILE_NAME = "iOSAppConfig.json";
    public const string ANDROID_FILE_NAME = "AndroidAppConfig.json";

    private const string iOS_File_LOAD_NAME = "iOSAppConfig";
    private const string ANDROID_FILE_LOAD_NAME = "AndroidAppConfig";

    public string BundleName { get; set; }
    public string ChannelName { get; set; }
    public string ProductName { get; set; }
    public bool Privacy { get; set; }
    public string MarketUrl { get; set; }
    public string CustomKey { get; set; }

    public string Lang { get; set; }

    public string VERSION { get; set; }

    public string GameIdentifier { get; set; }

    public bool IsChinese { get; private set; }

    public void Init()
    {

    }

    private static AppConfig LoadConfig()
    {
        AppConfig appConfig = null;

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
                {
                    appConfig = new DevAppConfig();
                    appConfig.IsChinese = appConfig.Lang == "zh";
                }
                break;
            default:
                {
                    string configName = string.Empty;
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        configName = ANDROID_FILE_LOAD_NAME;
                    }
                    else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        configName = iOS_File_LOAD_NAME;
                    }
                    TextAsset tx = Resources.Load<TextAsset>(configName);
                    if (tx != null)
                    {
                        Debug.Log(tx.text);
                        appConfig = JsonConvert.DeserializeObject<AppConfig>(tx.text);
                        appConfig.IsChinese = appConfig.Lang == "zh";
                    }
                }
                break;
        }

        return appConfig;
    }

    public void WriteConfig(string name)
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Resources/" + name);
        string jsonText = JsonConvert.SerializeObject(this); //JsonMapper.ToJson(this);
        System.IO.File.WriteAllText(path, jsonText, System.Text.Encoding.UTF8);
    }

    public bool IsDevelopmentChannel()
    {
        return this.ChannelName == "Development" || this.ChannelName == "iOSDevelopment";
    }

}

public class DevAppConfig : AppConfig
{
    public DevAppConfig()
    {
        ChannelName = "Development";
        Privacy = true;
        VERSION = "1.0.0";
        CustomKey = "9fTb77242@sZMy2%";
        Lang = "zh";
    }
}

public class iOSAppConfig : AppConfig
{
    public iOSAppConfig()
    {
        ChannelName = "iOS";
        Privacy = true;
    }
}
