using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class CommonButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Button btn;
    [SerializeField]
    private RectTransform clickerTrans;

    public Text mLabel;

    private Vector2 offsetMin;
    private Vector2 offsetMax;
    public Button.ButtonClickedEvent onClick { get; set; } = new Button.ButtonClickedEvent();

    private void Start()
    {
        offsetMin = clickerTrans.offsetMin;
        offsetMax = clickerTrans.offsetMax;

        btn.onClick.AddListener(() =>
        {
            onClick?.Invoke();
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clickerTrans.offsetMin = Vector2.zero;
        clickerTrans.offsetMax = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        clickerTrans.offsetMin = offsetMin;
        clickerTrans.offsetMax = offsetMax;
    }

}
