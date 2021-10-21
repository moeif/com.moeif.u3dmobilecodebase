using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

// 用来约束游戏的初始化流程
public class MoeGameStarter : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        InitGame();
    }

    private async void InitGame()
    {
#if UNITY_IOS
        iOS14AppTracking.ShowAppTracking();
#endif
        Debug.LogFormat("初始化表");
        await MoeGameConfigTables.InitTables();
        Debug.LogFormat("初始化EngineModule");
        await OnInitEngineModule();
        Debug.LogFormat("初始化LogicModule");
        await OnInitLogicModule();
        Debug.LogFormat("初始化 StartGameLogic");
        await OnStartGameLogic();
    }

    // 不同项目，初始化基础模块（与游戏逻辑无关模块）
    protected virtual async Task OnInitEngineModule()
    {

    }


    // 不同项目，初始化与游戏逻辑有关的模块
    protected virtual async Task OnInitLogicModule()
    {

    }

    // 开始游戏逻辑
    protected virtual async Task OnStartGameLogic()
    {

    }
}
