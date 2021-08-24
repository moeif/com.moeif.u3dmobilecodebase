using UnityEngine;
using System.Collections.Generic;

public class UIWidget : MonoBehaviour
{
    [System.Serializable]
    public class CompItem
    {
        public string Name;
        public GameObject Obj;
    }

    [HideInInspector]
    public List<CompItem> compItemList = new List<CompItem>();
    private Dictionary<int, CompItem> compItemDict = new Dictionary<int, CompItem>();

    private List<int> eventList = new List<int>();

    private RectTransform _rectTrans = null;
    public RectTransform rectTrans
    {
        get
        {
            if(_rectTrans == null)
            {
                _rectTrans = gameObject.GetComponent<RectTransform>();
            }
            return _rectTrans;
        }
    }


    private void Awake()
    {
        InitComps();
        OnInitCompos();
        OnAfterInitCompos();
    }

    protected virtual void Start()
    {
        OnStart();
    }

    private void InitComps()
    {
        for (int i = 0; i < compItemList.Count; ++i)
        {
            int key = compItemList[i].Name.GetHashCode();
            if (!compItemDict.ContainsKey(key))
            {
                compItemDict.Add(key, compItemList[i]);
            }
        }
    }

    protected virtual void OnInitCompos()
    {

    }

    protected virtual void OnAfterInitCompos()
    {

    }

    protected virtual void OnStart()
    {

    }


    public virtual void UpdateData()
    {

    }
      
    protected virtual void OnRefreshUI()
    {

    }



    private void OnDestroy()
    {
        if (MoeEventManager.GetInstance() != null)
        {
            foreach (int eventId in eventList)
            {
                MoeEventManager.Inst.UnRegisterEvent(eventId, this.ProcessEvent);
            }
        }

        eventList.Clear();
        eventList = null;
        OnClose();
    }

    protected virtual void OnClose()
    {

    }

    protected void RegisterEvent(int eventId)
    {
        if (!eventList.Contains(eventId))
        {
            eventList.Add(eventId);
            MoeEventManager.Inst.RegisterEvent(eventId, this.ProcessEvent);
        }
    }

    private void ProcessEvent(int eventId, MoeEventParam eventParam)
    {
        this.OnEvent(eventId, eventParam);
    }

    protected virtual void OnEvent(int eventId, MoeEventParam eventParam)
    {

    }

    protected void SendEvent(int eventId, MoeEventParam eventParam)
    {
        MoeEventManager.Inst.SendEvent(eventId, eventParam);
    }

    protected GameObject GetComp(string compName)
    {
        CompItem ci = null;
        compItemDict.TryGetValue(compName.GetHashCode(), out ci);
        return ci != null ? ci.Obj : null;
    }

    protected T GetComp<T>(string compName) where T : Component
    {
        CompItem ci = null;
        compItemDict.TryGetValue(compName.GetHashCode(), out ci);
        return ci.Obj.GetComponent<T>();
    }

}