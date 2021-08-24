using System.Collections;
using UnityEngine;

public class BaseItemWidget : UIWidget
{
    private object baseData = null;
    public void SetData(object data)
    {
            baseData = data;
            OnSetData(data);
    }

    protected virtual void OnSetData(object data)
    {

    }

    public virtual void RefreshUI()
    {

    }
}