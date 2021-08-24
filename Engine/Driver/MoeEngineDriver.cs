//#define UPDATE_ASSETS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MoeEngineDriver : MoeSingleton<MoeEngineDriver>
{
    protected override void InitOnCreate()
    {
        Application.targetFrameRate = 60;
    }


    // --------------------------------- C# Engine --------------------------------------
    private Action engineInitedCallback = null;
    public void InitEngine(Action engineInitedCallback)
    {
        this.engineInitedCallback = engineInitedCallback;

        Debug.LogFormat("AppConfig Init");
        AppConfig.Inst.Init();
        Debug.LogFormat("DataConfig Init");
        DataConfig.Inst.Init();
        Debug.LogFormat("MoeAnalyst Init");
        MoeAnalyst.Inst.Init();
        //Debug.LogFormat("MoeAD Init");
        MoeAD.Inst.Init();

        if (AppConfig.Inst.ChannelName == "Development" || AppConfig.Inst.ChannelName == "iOSDevelopment")
        {
            Debug.unityLogger.logEnabled = true;
        }
        else
        {
            Debug.unityLogger.logEnabled = false;
        }

        MoeEventManager.Inst.InitInstance();
        MoeResManager.Inst.InitInstance();

        engineInitedCallback?.Invoke();
    }
}
