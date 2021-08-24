using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DynamicScrollViewCell : MonoBehaviour
{
    private RectTransform rectTrans;

    private void Awake()
    {
        //rectTrans = gameObject.GetComponent<RectTransform>();
    }

    //public virtual void UpdateContent(int dataIndex)
    //{
    
    //}

    public void SetRectTrans(RectTransform rectTrans)
    {
        this.rectTrans = rectTrans;
    }

    public void UpdatePosition(Vector3 anchorPosition3D)
    {
        rectTrans.anchoredPosition3D = anchorPosition3D;
    }

}
