#if UNITY_ANDROID
using System.Collections;
using UnityEngine;
using Unity.Notifications.Android;

public class LocalNotification : MoeSingleton<LocalNotification>
{
    const string AndroidNotificationChannelId = "MoeifLotteryNotifAndroidId";
    const string AndroidNotificationChannelName = "Default";
    const int notifyId1 = 1992120901;
    const int notifyId2 = 1992120902;

    protected override void InitOnCreate()
    {
        ConstructAndroidChannel();
        ConstructAndroidNotification();
        ConstructAndroidNotification2();
    }

    private void ConstructAndroidChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = AndroidNotificationChannelId,
            Name = AndroidNotificationChannelName,
            Importance = Importance.High,
            Description = "LotteryFansNotifyChannel",
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    private void ConstructAndroidNotification()
    {
        var notification = new AndroidNotification();
        notification.Title = "福彩开奖通知";
        notification.Text = "福彩开奖数据已到达，赶快查看你的中奖情况吧~";
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";
        System.DateTime now = System.DateTime.Now;
        notification.FireTime = new System.DateTime(now.Year, now.Month, now.Day, 21, 40, 0);
        notification.RepeatInterval = new System.TimeSpan(1, 0, 0, 0, 0);
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, AndroidNotificationChannelId, notifyId1);
    }

    private void ConstructAndroidNotification2()
    {
        var notification = new AndroidNotification();
        notification.Title = "体彩开奖通知";
        notification.Text = "体彩开奖数据已到达，赶快查看你的中奖情况吧~";
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";
        System.DateTime now = System.DateTime.Now;
        notification.FireTime = new System.DateTime(now.Year, now.Month, now.Day, 20, 50, 0);
        notification.RepeatInterval = new System.TimeSpan(1, 0, 0, 0, 0);
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, AndroidNotificationChannelId, notifyId2);
    }
}
#endif
