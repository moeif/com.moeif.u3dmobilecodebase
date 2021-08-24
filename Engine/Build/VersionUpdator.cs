using System.Collections;
using UnityEngine;
using UniRx.Async;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class VersionInfo
{
    public string version { get; set; }
    public int num_version { get; set; }
    public string update_content { get; set; }
    public string[] data_url_list { get; set; }

    public void DebugInfo()
    {
        Debug.Log("=============");
        Debug.LogFormat("Version: {0}, numVersion: {1}", version, num_version);
        if(data_url_list != null)
        {
            foreach(string url in data_url_list)
            {
                Debug.LogFormat("{0}", url);
            }
        }
        Debug.Log("=============");
    }
}


public class VersionUpdator : MoeSingleton<VersionUpdator>
{
    private VersionInfo _versionInfo = null;
    public VersionInfo versionInfo
    {
        get
        {
            return _versionInfo;
        }
    }

    protected override void InitOnCreate()
    {
        Checking();
    }

    private string[] VERSION_INFO_URL = new string[]
    {
        "https://gitee.com/moeif/lfproject/raw/master/version.json",
        "https://moeif.com/apps/lotteryfans/version.json",
        "http://lotteryfansch1.moeif.com/apps/lotteryfans/version.json",
    };

    const string appstoreURL = "https://apps.apple.com/app/id1566518102";

    async void Checking()
    {
        VersionInfo versionInfo = null;
        int tryCount = 0;
        while(tryCount < 5 && versionInfo == null)
        {
            foreach(string url in VERSION_INFO_URL)
            {
                versionInfo = await CheckVersion(url);
                if(versionInfo != null)
                {
                    break;
                }
            }

            tryCount += 1;
        }

        if(versionInfo != null)
        {
            OnVersionInfoLoaded(versionInfo);
        }
    }

    async UniTask<VersionInfo> CheckVersion(string url)
    {
        Debug.LogFormat(">>>>>>>> RequestUrl: {0}", url);
        UnityWebRequest req = UnityWebRequest.Get(url);
        await req.SendWebRequest();
        if(req.result == UnityWebRequest.Result.Success)
        {
            string downloadedText = req.downloadHandler.text;
            try
            {
                //var versionInfo = JsonUtility.FromJson<VersionInfo>(downloadedText);
                var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(downloadedText);
                return versionInfo;
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        return null;
    }


    private void OnVersionInfoLoaded(VersionInfo versionInfo)
    {
        versionInfo.DebugInfo();
        _versionInfo = versionInfo;
        MoeEventManager.Inst.SendEvent(EventID.OnVersioinInfoLoaded);

        // 关于版本更新
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            return;
        }

        int myNumVersion = int.Parse(AppConfig.Inst.VERSION.Replace(".", ""));
        if (versionInfo.num_version > myNumVersion)
        {
            string[] newVersionArray = versionInfo.version.Split('.');
            string[] myVersionArray = AppConfig.Inst.VERSION.Split('.');

            if(newVersionArray[0] != myVersionArray[0] || newVersionArray[1] != myVersionArray[1])
            {
                DialogUpdate(true);
            }
            else
            {
                if(newVersionArray[2] != myVersionArray[2])
                {
                    DialogUpdate(false);
                }
            }
        }

    }

    private void DialogUpdate(bool forceUpdate = false)
    {
        string content = "发现新版本";
       
        string confirmText = "更新版本";
        string cancelText = forceUpdate ? string.Empty : "本次忽略";

        //UIQuestionBoxPanelData uiData = new UIQuestionBoxPanelData(confirmText, () =>
        //{
        //    Debug.Log("点击了更新版本");
        //    Application.OpenURL(appstoreURL);
        //},
        //cancelText,
        //() =>
        //{
        //    Debug.Log("点击了忽略更新");
        //},
        //content);

        //UIManager.OpenPanel(UIEnums.UIQuestionBoxPanel, 1, uiData);
    }
}