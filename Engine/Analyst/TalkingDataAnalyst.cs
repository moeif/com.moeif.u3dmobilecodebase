using System.Collections;
using UnityEngine;

#if TalkingDataAnalyst
public class TalkingDataAnalyst : MoeAnalyst
{
    private bool inited = false;
    
    protected override void OnStart()
    {
        if (!string.IsNullOrEmpty(DataConfig.Inst.TalkingDataAppId))
        {
            inited = true;
            TalkingDataGA.BackgroundSessionEnabled();
            TalkingDataGA.OnStart(DataConfig.Inst.TalkingDataAppId, AppConfig.Inst.ChannelName);
            TDGAProfile.SetProfile(SystemInfo.deviceUniqueIdentifier);
        }
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