using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class DataConfig
{
    private static DataConfig _inst = null;
    public static DataConfig Inst
    {
        get
        {
            if(_inst == null)
            {
                LoadConfig();
            }
            return _inst;
        }
    }

    public string TalkingDataAppId = string.Empty;
    public string UnityGameIdAndroid = string.Empty;
    public string UnityGameIdiOS = string.Empty;
    public string UnityGameIdAndroidEn = string.Empty;
    public string UnityGameIdiOSEn = string.Empty;

    const string FILE_DATA_CONFIG = "DataConfig";

    public void Init()
    {

    }

    private static void LoadConfig()
    {
        _inst = new DataConfig();
        TextAsset tx = Resources.Load<TextAsset>(FILE_DATA_CONFIG);
        if(tx != null)
        {
            JsonData jsonData = JsonMapper.ToObject(tx.text);
            if (jsonData.ContainsKey("TalkingDataAppId"))
            {
                _inst.TalkingDataAppId = jsonData["TalkingDataAppId"].ToString(); ;
            }

            if (jsonData.ContainsKey("UnityGameIdAndroid"))
            {
                _inst.UnityGameIdAndroid = jsonData["UnityGameIdAndroid"].ToString();
            }

            if(jsonData.ContainsKey("UnityGameIdiOS"))
            {
                _inst.UnityGameIdiOS = jsonData["UnityGameIdiOS"].ToString();
            }

            if(jsonData.ContainsKey("UnityGameIdAndroidEn"))
            {
                _inst.UnityGameIdAndroidEn = jsonData["UnityGameIdAndroidEn"].ToString();
            }

            if (jsonData.ContainsKey("UnityGameIdiOSEn"))
            {
                _inst.UnityGameIdiOSEn = jsonData["UnityGameIdiOSEn"].ToString();
            }
        }
    }


}