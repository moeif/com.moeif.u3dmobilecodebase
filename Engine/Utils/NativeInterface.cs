using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class NativeInterface
{
#if UNITY_IPHONE || UNITY_IOS
    [DllImport("__Internal")]
    public static extern bool iOSIsNotchScreen();

#endif

    public static bool IsNotchScreen()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return false;
#elif UNITY_IPHONE || UNITY_IOS
        return iOSIsNotchScreen();
#else
        return false;
#endif
    }
}
