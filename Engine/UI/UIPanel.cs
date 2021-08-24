using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UIPanel : UIWidget
{
    public CanvasGroup transGroup;
    public RectTransform contentRoot;
    public float transDuration = 0.3f;
    public int uiKey{ get; set; }
    public UILayer PanelLayer { get; set; } = UILayer.Panel;
    public int Signature { get; set; }

    public void SetData(object data = null)
    {
        Debug.LogFormat("==== 2 UIPanel {0} SetData", gameObject.name);
        this.data = data; 
    }

    protected override void Start()
    {
        base.Start();
        OpenUI(this.data);
    }

    protected object data = null;
    public void OpenUI(object data = null)
    {
        Debug.LogFormat("==== 3 UIPanel {0} OpenUI", gameObject.name);
        SetUIAdaptive();
        this.data = data;
        OnOpen();
        ActiveUI();
    }

    private void SetUIAdaptive()
    {
        if (contentRoot != null)
        {
            if (!NativeInterface.IsNotchScreen())
            {
                contentRoot.offsetMax = new Vector2(0, -64);
            }
        }
    }

    public void ActiveUI()
    {
        //UIFadeIn();
    }

    private void UIFadeIn()
    {
        if(transGroup != null)
        {
            transGroup.alpha = 0;
            transGroup.DOFade(1, transDuration);
        }
    }

    private void UIFadeOut()
    {
        if(transGroup != null)
        {
            transGroup.DOFade(0, transDuration);
        }
    }

    public object GetData()
    {
        return this.data;
    }

    protected virtual void OnOpen()
    {
        
    }

    public void CloseSelf()
    {
        if (transGroup != null)
        {
            UIFadeOut();
            StartCoroutine(DelayClose());
        }
        else
        {
            UIManager.Inst.CloseUI(this);
        }
    }

    private IEnumerator DelayClose()
    {
        GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = false;
        yield return new WaitForSeconds(transDuration);
        UIManager.Inst.CloseUI(this);
    }

    public void EnableCanvas()
    {
        gameObject.GetComponent<Canvas>().enabled = true;
    }

    public void DisableCanvas()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
    }
}