using System.Collections.Generic;
using UnityEngine;
using System;
using Itenso.TimePeriod;
using System.Text;

public static class MoeUtils
{
    public static readonly string[] DayOfWeek = new string[] { "日", "一", "二", "三", "四", "五", "六" };

    static System.Random rand = new System.Random();

    public static string GetDayOfWeek(int dayOfWeek)
    {
        return DayOfWeek[dayOfWeek];
    }

    public static DateTime UnixTimestamp2DateTime(long timestamp)
    {
        DateTime uStartTime = new DateTime(1970, 1, 1, 8, 0, 0);
        DateTime dateTime = uStartTime.AddSeconds(timestamp);
        return dateTime;
    }

    public static long DateTime2UnixTimestamp(DateTime time)
    {
        return (long)(time - new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds;
    }

    public static long GetNowTimestamp()
    {
        DateTime dt = DateTime.Now;
        return DateTime2UnixTimestamp(dt);
    }

    public static int DayDiffOfNow(DateTime dt)
    {
        DateTime now = DateTime.Now;
        now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        DateTime checkDate = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);

        TimeSpan ts = now - dt;
        return ts.Days;
    }

    public static DateTime GetFutureDate(DateTime from, int addDays)
    {
        DateTime _from = new DateTime(from.Year, from.Month, from.Day);
        return _from.AddDays(addDays);
    }

    public static void ShuffleArray<T>(T [] dataArray)
    {
        //System.Random rand = new System.Random();
        if (dataArray != null && dataArray.Length > 2)
        {
            int last = dataArray.Length - 1;
            for(int i = last; i >= 0; --i)
            {
                int selection = rand.Next(i + 1);

                T temp = dataArray[i];
                dataArray[i] = dataArray[selection];
                dataArray[selection] = temp;
            }
        }
    }

    public static void ShuffleList<T>(List<T> dataList)
    {
        
        if (dataList != null && dataList.Count > 2)
        {
            int last = dataList.Count - 1;
            for (int i = last; i >= 0; --i)
            {
                int selection = rand.Next(i + 1);

                T temp = dataList[i];
                dataList[i] = dataList[selection];
                dataList[selection] = temp;
            }
        }
    }

    public static void DebugList<T>(List<T> dataList)
    {
        StringBuilder builder = new StringBuilder(1024);
        for(int i = 0; i < dataList.Count; ++i)
        {
            builder.Append(dataList[i].ToString());
            builder.Append(", ");
        }

        Debug.Log(builder.ToString());
    }

    //public static string DateDiffNow(DateTime dt)
    //{
    //    DateTime now = DateTime.Now;

    //}


    //public static string TimeDiffWithNow(long timestamp)
    //{
    //    DateTime now = DateTime.Now;
    //    long nowTimestamp = DateTime2UnixTimestamp(now);
    //    if (nowTimestamp < timestamp)
    //    {
    //        return StrFuture;
    //    }
    //    else
    //    {
    //        DateTime date1 = UnixTimestamp2DateTime(timestamp);
    //        date1 = new DateTime(date1.Year, date1.Month, date1.Day);
    //        DateTime date2 = now;
    //        DateDiff dateDiff = new DateDiff(date1, date2);
    //        if (dateDiff.Days == 0)
    //        {
    //            return StrToday;
    //        }
    //        else
    //        {
    //            //if (dateDiff.Days == 1)
    //            //{
    //            //    return string.Format(GameDataUtil.GetLanguage(10006), dateDiff.Days);
    //            //}
    //            //else
    //            //{
    //            //    return string.Format(GameDataUtil.GetLanguage(10007), dateDiff.Days);
    //            //}

    //            if(dateDiff.Days > 0)
    //            {
    //                return 
    //            }
    //        }
    //    }
    //}

    public static bool IsRuntimeEditor()
    {
        return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor;
    }


}
