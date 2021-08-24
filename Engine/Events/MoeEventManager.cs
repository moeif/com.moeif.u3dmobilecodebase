using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void MoeEventCallback(int eventId, MoeEventParam param = null);
public class MoeEventManager : MoeSingleton<MoeEventManager>
{
    private List<MoeEvent> eventList = new List<MoeEvent>();
    private Dictionary<int, MoeEventCallback> eventCallbackDict = new Dictionary<int, MoeEventCallback>();
   
    public void RegisterEvent(int eventId, MoeEventCallback callback)
    {
        if (eventCallbackDict.ContainsKey(eventId))
        {
            eventCallbackDict[eventId] -= callback;
            eventCallbackDict[eventId] += callback;
        }
        else
        {
            eventCallbackDict.Add(eventId, callback);
        }
    }

    public void UnRegisterEvent(int eventId, MoeEventCallback callback)
    {
        if (eventCallbackDict.ContainsKey(eventId))
        {
            eventCallbackDict[eventId] -= callback;
            if (eventCallbackDict[eventId] == null)
            {
                eventCallbackDict.Remove(eventId);
            }
        }
    }

    public void SendEvent(int eventId, MoeEventParam param = null)
    {
        eventList.Add(new MoeEvent(eventId, param));
    }

    public void SendEventNow(int eventId, MoeEventParam param = null)
    {
        ConsumeEvent(eventId, param);
    }

    private void Update()
    {
        for (int i = 0; i < eventList.Count; ++i)
        {
            MoeEvent moeEvent = eventList[i];
            ConsumeEvent(moeEvent.eventId, moeEvent.param);
        }

        eventList.Clear();
    }

    private void ConsumeEvent(int eventId, MoeEventParam param)
    {
        MoeEventCallback callbacks = null;
        if (eventCallbackDict.TryGetValue(eventId, out callbacks))
        {
            callbacks(eventId, param);
        }

#if MOE_XLUA
        NotifyToLua(eventId, param);
#endif
    }


#if MOE_XLUA
    private void NotifyToLua(int eventId, object param)
    {
        MoeLuaBridge.Inst.NotifyToLua(eventId, param);
    }
#endif


    public void Reset()
    {
        eventList.Clear();
        eventCallbackDict.Clear();
    }


}