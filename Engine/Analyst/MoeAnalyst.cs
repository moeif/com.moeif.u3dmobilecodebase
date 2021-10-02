using System.Collections;
using UnityEngine;

public class MoeAnalyst : MonoBehaviour
{
    private static MoeAnalyst _inst = null;
    public static MoeAnalyst Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject obj = new GameObject("MoeAnalyst");
                DontDestroyOnLoad(obj);
#if TalkingDataAnalyst
                _inst = obj.AddComponent<TalkingDataAnalyst>();
#else
                _inst = obj.AddComponent<MoeAnalyst>();
#endif
            }

            return _inst;
        }
    }

    public void Init(MoeModuleData mmd)
    {
        OnInit(mmd);
    }

    protected virtual void OnInit(MoeModuleData mmd)
    {

    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void OnStart()
    {

    }

    public virtual void TrackEvent(string eventName)
    {

    }


}