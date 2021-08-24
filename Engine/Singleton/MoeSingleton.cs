using System.Collections;
using UnityEngine;

public class MoeSingleton<T> : MonoBehaviour where T : MoeSingleton<T>
{

    private static T _inst = null;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject obj = new GameObject (typeof (T).Name);
                DontDestroyOnLoad (obj);
                _inst = obj.AddComponent<T> ();
                _inst._MoeInit ();
            }
            return _inst;
        }
    }

    public static T GetInstance()
    {
        return _inst;
    }

    private void Awake ()
    {
        if (_inst == null)
        {
            _inst = this as T;
            gameObject.name = typeof(T).Name;
            DontDestroyOnLoad(gameObject);
            _inst._MoeInit ();
        }
    }

    private bool inited = false;

    private void _MoeInit()
    {
        if (!inited)
        {
            inited = true;
            InitOnCreate();
        }
    }

    protected virtual void InitOnCreate () { }

    public void InitInstance () { }
}