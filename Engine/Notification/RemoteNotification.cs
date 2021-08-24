//using System.Collections;
//using UnityEngine;

//public class RemoteNotification : MoeSingleton<RemoteNotification>
//{

//    string strToken = null;
//    bool tokenSent;
//    private void Start()
//    {
//        tokenSent = false;
//        UnityEngine.iOS.NotificationServices.RegisterForNotifications(
//            UnityEngine.iOS.NotificationType.Alert |
//            UnityEngine.iOS.NotificationType.Badge |
//            UnityEngine.iOS.NotificationType.Sound);//---------远程推送注册

//    }
//    private void Update()
//    {
//        if (!tokenSent)
//        {
//            byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
//            if (token != null)
//            {
//                strToken = System.BitConverter.ToString(token);//这里获取到的格式是A1-A2-A3这种格式，服务器推送需要的没有-
//                strToken = strToken.Replace("-", "");
//                tokenSent = true;
//            }
//        }
//    }
//}