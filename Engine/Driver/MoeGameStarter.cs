using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

// 用来约束游戏的初始化流程
public class MoeGameStarter : MonoBehaviour
{
    private void Awake()
    {
        InitGame();
    }

    private async void InitGame()
    {
        await MoeGameConfigTables.InitTables();
        await OnInitEngineModule();
        await OnInitLogicModule();
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
