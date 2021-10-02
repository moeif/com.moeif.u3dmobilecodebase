using System.Collections;
using UnityEngine;

#if TalkingDataAnalyst

public class TalkingDataModuleData : MoeModuleData {
    public string appId;
    public string channelName;

    public TalkingDataModuleData(string appId, string channelName){
        this.appId = appId;
        this.channelName = channelName;
    }
}

public class TalkingDataAnalyst : MoeAnalyst
{
    private bool inited = false;

    protected override void OnInit(MoeModuleData mmd){
        TalkingDataModuleData tdmd = mmd as TalkingDataModuleData;
        if(!inited){
            if (!string.IsNullOrEmpty(tdmd.appId))
            {
                inited = true;
                TalkingDataGA.BackgroundSessionEnabled();
                TalkingDataGA.OnStart(tdmd.appId, tdmd.channelName);
                TDGAProfile.SetProfile(SystemInfo.deviceUniqueIdentifier);
            }
        }
    }
    
    protected override void OnStart()
    {
        
    }

    private void OnDestroy()
    {
        if (inited)
        {
            TalkingDataGA.OnEnd();
        }
    }

    public override void TrackEvent(string eventName)
    {
        if (inited)
        {
            TalkingDataGA.OnEvent(eventName, null);
        }
    }
}
#endif