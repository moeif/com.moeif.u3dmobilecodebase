using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoeAssetUpdateUI : MoeSingleton<MoeAssetUpdateUI>
{
    public Image mProgressImage;
    
    public void UpdateProgress(long downloadedSize, long needDownloadSize)
    {
        if(mProgressImage != null)
        {
            float percent = (float)downloadedSize / (float)needDownloadSize;
            percent = Mathf.Clamp(percent, 0, 1);
            mProgressImage.fillAmount = percent;
        }

    }

    public void OnDownloadFaild()
    {

    }

    public void OnUpdateComplete()
    {
        mProgressImage.fillAmount = 1;
    }


}