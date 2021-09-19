//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//public class Starter : MoeSingleton<Starter>
//{
//    public GameObject fpsObj = null;

//    protected override void InitOnCreate()
//    {
//        MoeEngineDriver.Inst.InitEngine(() =>
//        {
//            FPSSwitch();
//            StartLogic();
//        });
//    }

//    private void FPSSwitch()
//    {
//        if(fpsObj != null)
//        {
//            fpsObj.SetActive(AppConfig.Inst.IsDevelopmentChannel());
//        }
//    }

//    private Action ActionForShowVersionUpdate = null;
//    private async void Setp_VersionInfo()
//    {
//        Debug.LogFormat("进入服务器状态和版本检查流程");
//        NetServerInfo serverInfo = null;
//        int reqCount = 0;
//        while (serverInfo == null && reqCount < 3)
//        {
//            Debug.LogFormat("拉取版本信息，尝试 {0}", reqCount);
//            serverInfo = await GameNet.Inst.GetServerInfo();
//            reqCount += 1;
//        }

//        if (serverInfo == null)
//        {
//            Debug.LogFormat("版本信息3次拉取失败，弹出网络有问题的框");
//            // 网络有问题吧，弹出个提示框来，玩家点确定后，继续尝试
//            UIMessagePanelData uiData = new UIMessagePanelData
//            {
//                msg = Utils.GetLanguage(10039),
//                confirmText = Utils.GetLanguage(10040),
//                confirmCallback = () =>
//                {
//                    Setp_VersionInfo();
//                }
//            };
//            UIManager.OpenPanel(UIEnums.UIMessagePanel, 1, uiData);
//        }
//        else
//        {
//            Debug.LogFormat("版本信息拉取成功，处理版本信息");
//            if (HandleServerStatus(serverInfo))
//            {
//                Step_InitPlayerData();
//            }
//        }
//    }

//    private void Step_InitPlayerData()
//    {
//        Debug.LogFormat("进入初始化用户数据流程");
//        NetUserInfo userInfo = new NetUserInfo();
//        if (!userInfo.LoadFromLocal())
//        {
//            Debug.LogFormat("用户数据从本地拉取失败，现在尝试从服务器获取");
//            Step_InitDataFromServer();
//        }
//        else
//        {
//            Debug.LogFormat("用户数据从本地读取成功，继续读取本地其他数据");
//            // 本地数据加载成功，进行赋值
//            GameModels.Inst.playerData.UpdatePlayerData(userInfo.name);

//            // 加载道具数据
//            Debug.LogFormat("从本地加载道具数据");
//            GameModels.Inst.itemsData.LoadFromLocal();

//            // 加载广告福利商店数据
//            Debug.LogFormat("从本地加载福利商店数据");
//            GameModels.Inst.fortuneStoreData.LoadFromLocal();

//            // 加载RankPlay数据
//            Debug.LogFormat("从本地加载冲榜数据");
//            GameModels.Inst.gamesData.rankGameData.LoadFromLocal();

//            // 加载MatchPlay数据
//            Debug.LogFormat("从本地加载匹配玩法数据");
//            GameModels.Inst.gamesData.matchGameData.LoadFromLocal();

//            // 加载关卡数据
//            Debug.LogFormat("从本地加载关卡数据");
//            GameModels.Inst.gamesData.soloGameData.LoadFromLocal();

//            Debug.LogFormat("加载上次游戏时间");
//            GameModels.Inst.powerRegainData.LoadFromLocal();
//            GameModels.Inst.powerRegainData.TryRegainPower();
            
//            EnterGame();
//        }

//    }

//    private async void Step_InitDataFromServer()
//    {
//        Debug.LogFormat("进入从服务器获取用户数据流程");
//        // 本地数据丢失，需要从网络加载
//        NetUserInfo userInfo = null;
//        int retryCount = 0;
//        while (userInfo == null && retryCount < 3)
//        {
//            Debug.LogFormat("从服务器获取用户数据，第 {0} 次尝试", retryCount);
//            userInfo = await GameNet.Inst.GetUserInfo();
//            retryCount += 1;
//        }

//        if (userInfo == null)
//        {
//            Debug.LogFormat("从服务器获取用户数据失败，可能是网络有问题，弹窗！");
//            // 网络提示
//            // 网络有问题吧，弹出个提示框来，玩家点确定后，继续尝试
//            UIMessagePanelData uiData = new UIMessagePanelData
//            {
//                msg = Utils.GetLanguage(10039),
//                confirmText = Utils.GetLanguage(10040),
//                confirmCallback = () =>
//                {
//                    Step_InitDataFromServer();
//                }
//            };
//            UIManager.OpenPanel(UIEnums.UIMessagePanel, 1, uiData);
//        }
//        else
//        {
//            Debug.LogFormat("成功从服务器获取到用户数据");
//            if (!userInfo.IsOk())
//            {
//                Debug.LogFormat("没有从服务器获取到当前设备的玩家数据，这是个新玩家");
//                // 然后EnterGame
//                UIManager.OpenPanel(UIEnums.UINewPlayerPanel);
//            }
//            else
//            {
//                GameModels.Inst.playerData.UpdatePlayerData(userInfo.name);
//                userInfo.SaveToLocal();
//                Debug.LogFormat("从服务器获取到用户数据OK，需要进行数据恢复!");
//                // 服务器有数据，执行数据恢复逻辑
//                Step_RestoreDataFromServer();
//            }

//        }
//    }

//    private async void Step_RestoreDataFromServer()
//    {
//        Debug.LogFormat("进入从服务器回复数据流程");
//        // 服务器有数据，需要进行数据恢复
//        NetRestoreInfo restoreInfo = null;
//        int retryCount = 0;
//        while (restoreInfo == null && retryCount < 3)
//        {
//            Debug.LogFormat("从服务器恢复数据，第 {0} 次尝试", retryCount);
//            restoreInfo = await GameNet.Inst.GetRestoreInfo();
//            retryCount += 1;
//        }

//        if (restoreInfo == null)
//        {
//            Debug.LogFormat("从服务器恢复数据网络请求失败，可能是网络问题，弹窗!");
//            // 网络有问题吧
//            UIMessagePanelData uiData = new UIMessagePanelData
//            {
//                msg = Utils.GetLanguage(10039),
//                confirmText = Utils.GetLanguage(10040),
//                confirmCallback = () =>
//                {
//                    Step_RestoreDataFromServer();
//                }
//            };
//            UIManager.OpenPanel(UIEnums.UIMessagePanel, 1, uiData);
//        }
//        else
//        {
//            Debug.LogFormat("从服务器恢复数据成功，设置数据，存本地，进入游戏！");
//            // 数据恢复
//            GameModels.Inst.gamesData.matchGameData.RestoreFromServer(restoreInfo.match_play_info);
//            GameModels.Inst.gamesData.rankGameData.RestoreFromServer(restoreInfo.rank_play_info);
//            GameModels.Inst.itemsData.RestoreFromServer(restoreInfo.items_info);
//            EnterGame();
//        }
//    }


//    public void EnterGame()
//    {
//        Debug.LogFormat("正式进入游戏");
//        // 优化体验，如果弹隐私，就不弹版本更新了，否则在主界面弹版本更新

//        if (AppConfig.Inst.Privacy && UIPrivacyPanel.NeedShowPrivacy())
//        {
//            UIManager.OpenPanel(UIEnums.UIPrivacyPanel, 0);
//            return;
//        }

//        EnterGameAfterPrivacy();
//    }

//    public void EnterGameAfterPrivacy(bool fromPrivacy = false)
//    {
//        UIManager.OpenPanel(UIEnums.UIMainPanel);
//        MoeEventManager.Inst.SendEvent(EventID.OnSystemInited);

//        if (!fromPrivacy)
//        {
//            ActionForShowVersionUpdate?.Invoke();
//        }
//    }


//    private async void StartLogic()
//    {
//        UIManager.OpenPanel(UIEnums.UILoadingPanel);

//        // 打开加载界面
//        await GameConfigTables.InitTables();
//        Debug.LogFormat("数据表加载完成");

//        Setp_VersionInfo();

//        HeadIconConfig.LoadTextures();

//        //NetUserInfo userInfo = await GameNet.Inst.GetUserInfo(); // 如果获取到的用户信息为空，则说明是新用户，弹出输入昵称界面
//        //if(userInfo != null && userInfo.IsOk())
//        //{
//        //    userInfo.ShowInfo();
//        //}

//        //await GameNet.Inst.PutUserInfo(new NetUserInfo
//        //{
//        //    id = GameModels.Inst.playerData.uid,
//        //    name = "我的名字是诗仙",
//        //    lang = "zh" //AppConfig.Inst.Lang
//        //});

//        //await GameNet.Inst.PutLevelInfo(new NetLevelInfo(1, 12));
//        //await GameNet.Inst.PutLevelInfo(new NetLevelInfo(3, 6));

//        //await GameNet.Inst.PutMatchPlayInfo(new NetMatchPlayInfo(1, 0, 0, 0));

//        //NetMatchPlayInfo matchPlayInfo = await GameNet.Inst.GetMatchPlayInfo();
//        //if (matchPlayInfo != null && matchPlayInfo.IsOk())
//        //{
//        //    matchPlayInfo.ShowInfo();
//        //}
//        //else
//        //{
//        //    Debug.LogFormat("No Match PlayInfo");
//        //}


//        //await GameNet.Inst.PutRankPlayInfo(new NetRankPlayInfo(8876));

//        //NetRankPlayInfo rankPlayInfo = await GameNet.Inst.GetRankPlayInfo();
//        //if (rankPlayInfo == null)
//        //{
//        //    Debug.LogErrorFormat("网络错误");
//        //}
//        //else
//        //{
//        //    if (rankPlayInfo.IsOk())
//        //    {
//        //        rankPlayInfo.ShowInfo();
//        //    }
//        //    else
//        //    {
//        //        Debug.LogFormat("没有RankPlayInfo的数据");
//        //    }
//        //}

//        //NetRestoreInfo restoreInfo = await GameNet.Inst.GetRestoreInfo();

//        //await GameNet.Inst.PutItemsInfo(new NetItemsInfo(100, 1, 0));

//        //NetItemsInfo itemsInfo = await GameNet.Inst.GetItemsInfo();
//        //if(itemsInfo != null)
//        //{
//        //    itemsInfo.ShowInfo();
//        //}


//        // 拉取版本信息
//        // 执行版本检测逻辑
//        // 拉取服务器信息
//        // 执行链接服务器逻辑


        

//        //UIManager.OpenPanel(UIEnums.UISoloPlayPanel);

//        //FakeData();


//#if UNITY_ANDROID
//        //LocalNotification.Inst.InitInstance();
//#elif UNITY_IPHONE && ONESIGNAL
//        //OneSignalNotification.Inst.InitInstance();
//#endif
//    }

//    private bool HandleServerStatus(NetServerInfo serverInfo)
//    {
//        if(serverInfo.server_status != "ok")
//        {
//            // 服务器维护，弹出个提示来，不过一般不会有这种情况
//        }

//        BaseGame.RankServerUrl = serverInfo.rank_server;
//        BaseGame.MatchServerUrl = serverInfo.match_server;

//        string[] appVersionNumStr = AppConfig.Inst.VERSION.Split('.');
//        int mainVersion = int.Parse(appVersionNumStr[0]);
//        int subVersion = int.Parse(appVersionNumStr[1]);

//        // 创建一个闭包，后面延迟调用
//        ActionForShowVersionUpdate = () =>
//        {
//            if (Application.platform == RuntimePlatform.Android)
//            {
//                //if (AppConfig.Inst.VERSION != serverInfo.android_version)
//                //{
//                    string[] serverVersionNumStr = serverInfo.android_version.Split('.');
//                    int serverMainVersion = int.Parse(serverVersionNumStr[0]);
//                    int serverSubVersion = int.Parse(serverVersionNumStr[1]);

//                    if (serverMainVersion > mainVersion || serverSubVersion > subVersion)
//                    {
//                        // Android 有新版本
//                        if (!string.IsNullOrEmpty(AppConfig.Inst.MarketUrl))
//                        {
//                            ShowVersionUpdateMsg(serverInfo.android_version);
//                        }
//                    }
//                //}
//            }
//            else if (Application.platform == RuntimePlatform.IPhonePlayer)
//            {
//                ////if (AppConfig.Inst.VERSION != serverInfo.ios_version)
//                //{
//                    string[] serverVersionNumStr = serverInfo.ios_version.Split('.');
//                    int serverMainVersion = int.Parse(serverVersionNumStr[0]);
//                    int serverSubVersion = int.Parse(serverVersionNumStr[1]);

//                    if (serverMainVersion > mainVersion || serverSubVersion > subVersion)
//                    {
//                        if (!string.IsNullOrEmpty(AppConfig.Inst.MarketUrl))
//                        {
//                            // IOS有新版本
//                            ShowVersionUpdateMsg(serverInfo.ios_version);
//                        }
//                    }
//                //}
//            }
//        };

//        return true;
//    }

//    private void ShowVersionUpdateMsg(string version)
//    {
//        UIMessagePanelData uiData = new UIMessagePanelData
//        {
//            msg = string.Format(Utils.GetLanguage(10041), version),
//            confirmText = Utils.GetLanguage(10042),
//            cancelText = Utils.GetLanguage(10043),
//            confirmCallback = () =>
//            {
//                Application.OpenURL(AppConfig.Inst.MarketUrl);
//            },
//        };

//        UIManager.OpenPanel(UIEnums.UIMessagePanel, 1, uiData);
//    }

    
//    private void DoTest()
//    {
//        //string param = "{\"itype\": 1, \"latest\": 2021057}";
//        //string aesbase64 = MoeCrypto.EncryptToAesBase64(param);
//        //string urlencoded = UnityEngine.Networking.UnityWebRequest.EscapeURL(aesbase64);
//        //Debug.Log(aesbase64);
//        //Debug.Log(urlencoded);
//        ////Debug.Log(aesbase64);
//        ////Debug.Log(MoeCrypto.DecryptFromAesBase64(aesbase64));
//        //string str = LotteryDataUpdator.GetEncryptedBase64ReqStr((int)EnLotteryType.SSQ, "10101");
//        //Debug.Log(str);

//        //string encrypted = "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";
//        //Debug.Log(encrypted.Length);
//        //Debug.Log(MoeCrypto.DecryptFromAesBase64(encrypted));

//        //for (int i = 0; i < 10; ++i)
//        //{
//        //    int maxScore = Random.Range(0, 1200000);
//        //    GameModels.Inst.poemData.GetRandomLevels(maxScore, 10);
//        //}

//        StartCoroutine(GeneratePoemSignCSV());
//    }

//    private IEnumerator GeneratePoemSignCSV()
//    {
//        List<string> csvLineList = new List<string>();
//        List<Poem> poemList = PoemConfigTable.Inst.GetAllPoem();
//        foreach (Poem poem in poemList)
//        {
//            PoemWrapper pWrapper = new PoemWrapper(poem);

//            for (int i = 0; i < pWrapper.singleLineList.Count; ++i)
//            {
//                string singleLine = pWrapper.singleLineList[i];
//                int length = singleLine.Length;
//                if (length > 1)
//                {
//                    int lineSign = poem.id * 1000 + i;
                    
//                    for(int num = 0; num < 10; ++num)
//                    {
//                        List<int> randSelectionSignList = GameModels.Inst.poemData.GetRandomSelectionSignList(poem.id, i, length);
//                        int sign1 = randSelectionSignList[0];
//                        int sign2 = randSelectionSignList[1];
//                        int sign3 = randSelectionSignList[2];
//                        int sign4 = randSelectionSignList[3];
//                        string csvLine = string.Format("{0},{1},{2},{3},{4},{5},{6}", poem.id, poem.id, lineSign, sign1, sign2, sign3, sign4);
//                        csvLineList.Add(csvLine);
//                    }
//                }
//            }
//            Debug.LogFormat("Poem: {0}", poem.id);
//            yield return null;
//        }

//        string output = "D:/moeif/poem-challenge/poem.csv";
//        System.IO.File.WriteAllLines(output, csvLineList);
//    }

//}
